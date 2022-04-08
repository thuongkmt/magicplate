
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IucBrain
{
    using Konbi.Common.Helpers;
    using Konbi.Common.Interfaces;
    using KonbiBrain.WindowServices.IUC.Interfaces;
    using KonbiBrain.WindowServices.IUC.Iuc;
    using Newtonsoft.Json;
    using System.Configuration;
    using static Thread;

    public class IucSerialPortInterface : Object,IDisposable, IIucDeviceService
    {
        #region Serial Port
        private readonly IKonbiBrainLogService _logger;
        public readonly Guid InstanceId = Guid.NewGuid();

        SerialPort Port;
        private static int INumber { get; set; }
        private StringBuilder _cmdBuilder = new StringBuilder();


        public Action<string> Debug { get; set; }

        public Action<object,IucApprovedResponse> OnSaleApproved { get; set; }
        public Action<object,SaleResponse> OnSaleError { get; set; }
        public Action<object, SaleResponse> OnSaleCancelled { get; set; }
        public Action<object, ResponseBase> OnTerminalCallback { get; set; }

        private ConcurrentQueue<string> _responseQueue = new ConcurrentQueue<string>();
        private ConcurrentQueue<string> _commandQueue = new ConcurrentQueue<string>();
        private DateTime? lastSaleDateTime = null;
        public string ComportName { get; set; }
        public IucPaymentMode Mode { get; set; }
        public bool DebugMode
        {
            get
            {
                return true;
            }
        }

        private bool isTerminalRunning;
        private bool cancelTransactionSilently = false;

        public bool IsTerminalRunning
        {
            get
            {
                return isTerminalRunning;
            }
            set
            {
                if (isTerminalRunning != value)
                {
                    isTerminalRunning = value;
                }
            }
        }
        public IucSerialPortInterface(IKonbiBrainLogService logger)
        {
            _logger = logger;
        }
        public bool ConnectPort(string port, System.Action<string> action = null)
        {
            try
            {
                INumber = -1;
                Port = new SerialPort(port)
                {
                    BaudRate = 9600,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    DtrEnable = true,
                };
                Port.DataReceived += Port_DataReceived;
                Port.Open();

                if (!Port.IsOpen) return false;

                ComportName = port;

                var msg = string.Empty;
                if (Port.IsOpen)
                {
                    msg = $"Device not found at [{ComportName}]";
                    if (CheckDevice())
                    {
                        msg = $"Found iUC unit: [{ComportName}]";
                    }
                }
                else
                {
                    msg = "Port is close";
                }

                LogIucApi(msg);
                action?.Invoke(msg);


                return Port.IsOpen;
            }
            catch (Exception ex)
            {
                LogIucApi(ex.ToString());

            }

            return false;
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!Port.IsOpen) return;

            try
            {
                var buffer = new byte[Port.BytesToRead];
                var bytesRead = Port.Read(buffer, 0, Port.BytesToRead);
                if (bytesRead > 0)
                {
                    RaiseAppSerialDataEvent(buffer);
                }
            }
            catch (Exception ex)
            {                
                LogIucApi($"Port_DataReceived: {ex.Message}");
            }
        }


      
        private void RaiseAppSerialDataEvent(byte[] bytes)
        {
            var dataOnThisSession = new List<byte>();
            foreach (var Data in bytes)
            {
                QueueResponse(new byte[] { Data }.ByteToHex().Trim());
                if (Data == 0x06)
                {
                    LogIucApi("<---- ACK");
                    return;
                }
                if (Data == 0x15)
                {
                    LogIucApi("<---- NACK");
                    return;
                }
                if (Data == 0x05)
                {
                    LogIucApi("<---- EQN");
                    this.Ack();
                    return;
                }
                if (Data == 0x04)
                {
                    LogIucApi("<---- EOT");
                    return;
                }

                dataOnThisSession.Add(Data);
            }

            if (DebugMode)
            {
                LogIucApi("<<< " + dataOnThisSession.ToArray().ToHexString());
            }
            _cmdBuilder.Append(dataOnThisSession.ToArray().ToHexString().Trim());

            if (_cmdBuilder.ToString().Trim().StartsWith("02"))
            {
                var endIndex = _cmdBuilder.ToString().Trim().IndexOf("03");
                if (endIndex >= 0)
                {
                    var cmd = _cmdBuilder.ToString().Substring(0, endIndex + 5);
                    _cmdBuilder = _cmdBuilder.Remove(0, cmd.Length);
                    if (DebugMode)
                    {
                        LogIucApi("LEFT:" + _cmdBuilder + "|");
                    }
                    ParseData(cmd.Trim().Replace(" ", string.Empty).ToHexBytes());
                }
            }
        }



        private void WritePortData(byte[] bytes)
        {
            if (Port == null|| string.IsNullOrEmpty(ComportName))
            {
                LogIucApi("Serial Port is not initialized yet. HALT");
                return;
            }
            LogIucApi("===================================SENDING COMMAND===================================");
            LogIucApi($"----> {bytes.ToHexString()}");
            var rawData = bytes.ToList();

            if (bytes[0] == 0x02)
            {
                // Remove STX
                rawData.RemoveAt(0);
                // Remove LRC
                rawData.RemoveAt(rawData.Count - 1);
                // Remove ETX
                rawData.RemoveAt(rawData.Count - 1);
                var stringData = rawData.ToArray().ToAsiiString();
                LogIucApi("----> " + stringData);
            }


            if (Port.IsOpen)
            {
                Port.Write(bytes, 0, bytes.Length);
            }
            else
            {
                LogIucApi("Port is closed!. Try opening ...");
                try
                {
                    Port.Open();
                    // Resend data when Port opens again.
                    LogIucApi("Port  reopen success. retry to send out the command.");
                    WritePortData(bytes);
                }
                catch (Exception ex)
                {

                    LogIucApi($"Exception: {ex.Message}");
                }
                
            }
        }


        #endregion

        #region Parse Data
        private void ParseData(byte[] data)
        {

            LogIucApi("===================================RECEIVING COMMAND===================================");
            LogIucApi("<---- " + data.ToHexString());
            var rawData = data.ToList();

            // Remove STX
            rawData.RemoveAt(0);
            // Remove LRC
            rawData.RemoveAt(rawData.Count - 1);
            // Remove ETX
            rawData.RemoveAt(rawData.Count - 1);

            var response = rawData.ToArray().ToAsiiString();
            LogIucApi("<---- " + response);
            this.Ack();

            ProcessResponseData(response);
        }


        private void ProcessResponseData(string response)
        {
            QueueResponse(response);

            if (string.Compare(response, 0, "R923", 0, 4) == 0)
            {
                Task.Run(() =>
                {
                // Callback
                GetTagValue(response, 34, out string outValue);
                    var code = outValue.Substring(0, 2);
                    var message = outValue.Replace(code, string.Empty);
                    var callbackResponse = new ResponseBase(Mode) { Message = message, ResponseCode = code };
                    LogIucApi($"Callback: {JsonConvert.SerializeObject(callbackResponse)}");
                    OnTerminalCallback?.Invoke(this,callbackResponse);
                });
            }

            if (string.Compare(response, 0, "R20", 0, 3) == 0 ||
             string.Compare(response, 0, "R630", 0, 4) == 0 ||
             string.Compare(response, 0, "R300", 0, 4) == 0 ||
             string.Compare(response, 0, "R600", 0, 4) == 0 ||
             string.Compare(response, 0, "R610", 0, 4) == 0)

            {
                if (GetTagValue(response, 39, out string responseCode) == 0)
                {
                    GetTagValue(response, 4, out string amount);
                    var saleReponse = new SaleResponse(Mode)
                    {
                        Amount = int.Parse(amount),
                        ResponseCode = responseCode,
                        Message = IucErrorCode.CResponseCode(responseCode)
                    };

                    var approvedResponse = new IucApprovedResponse(Mode);

                    if (string.CompareOrdinal(responseCode, "00") == 0)
                    {
                        LogIucApi("SALE APPROVED");

                        #region Transaction Infomations

                        var transInfo = "";
                        transInfo += "-----------------------------------\r\n";
                        if (string.Compare(response, 0, "R200", 0, 4) == 0)
                            transInfo += "              SALE\r\n";
                        else if (string.Compare(response, 0, "R600", 0, 4) == 0)
                            transInfo += "              SALE\r\n";
                        else if (string.Compare(response, 0, "R630", 0, 4) == 0)
                            transInfo += "              SALE\r\n";
                        else if (string.Compare(response, 0, "R201", 0, 4) == 0)
                            transInfo += "            PREAUTH\r\n";
                        else if (string.Compare(response, 0, "R202", 0, 4) == 0)
                            transInfo += "            OFFLINE\r\n";
                        else if (string.Compare(response, 0, "R203", 0, 4) == 0)
                            transInfo += "             REFUND\r\n";
                        else if (string.Compare(response, 0, "R300", 0, 4) == 0)
                            transInfo += "             VOID\r\n";
                        else
                            transInfo += "           TRANSACTION\r\n";
                        transInfo += "-----------------------------------\r\n";

                        GetTagValue(response, 41, out var stValue);
                        transInfo += "TID: ";
                        transInfo += stValue;
                        approvedResponse.Tid = stValue;

                        transInfo += "  ";
                        GetTagValue(response, 42, out stValue);
                        transInfo += "MID: ";
                        transInfo += stValue;
                        transInfo += "\r\n";
                        approvedResponse.Mid = stValue;

                        if (GetTagValue(response, 7, out stValue) == 0)
                        {
                            var dateTime = string.Empty;
                            transInfo += "DATE TIME: ";
                            dateTime += stValue.Substring(2, 2); // DD   
                            dateTime += "/";
                            dateTime += stValue.Substring(0, 2); // MM
                            dateTime += " ";
                            dateTime += stValue.Substring(4, 2);
                            dateTime += ":";
                            dateTime += stValue.Substring(6, 2);
                            dateTime += ":";
                            dateTime += stValue.Substring(8, 2);
                            transInfo += dateTime;
                            transInfo += "\r\n";

                            approvedResponse.DateTime = dateTime;
                        }

                        if (GetTagValue(response, 62, out stValue) == 0)
                        {
                            transInfo += "INVOICE: ";
                            transInfo += stValue;
                            approvedResponse.Invoice = stValue;
                            transInfo += "  ";
                        }
                        if (GetTagValue(response, 64, out stValue) == 0)
                        {
                            transInfo += "BATCH: ";
                            approvedResponse.Batch = stValue;
                            transInfo += stValue;
                        }
                        transInfo += "\r\n";

                        if (GetTagValue(response, 61, out stValue) == 0)
                        {
                            transInfo += "UTRN: ";
                            transInfo += stValue;
                            transInfo += "\r\n";
                        }

                        if (GetTagValue(response, 54, out stValue) == 0)
                        {
                            // card label
                            transInfo += stValue;
                            approvedResponse.CardLabel = stValue;
                            transInfo += ": ";
                        }
                        if (GetTagValue(response, 2, out stValue) == 0)
                        {
                            // card number
                            transInfo += stValue;
                            approvedResponse.CardNumber = stValue;
                            transInfo += "\r\n";
                        }

                        if (GetTagValue(response, 37, out stValue) == 0)
                        {
                            transInfo += "RRN: ";
                            transInfo += stValue;
                            approvedResponse.Rrn = stValue;
                            transInfo += "\r\n";
                        }

                        if (GetTagValue(response, 38, out stValue) == 0)
                        {
                            transInfo += "APPROVAL CODE: ";
                            transInfo += stValue;
                            approvedResponse.ApproveCode = stValue;
                            transInfo += "\r\n";
                        }

                        if (GetTagValue(response, 22, out stValue) == 0)
                        {
                            transInfo += "ENTRY MODE: ";
                            if (stValue[0] == 'E')
                                transInfo += "MANUAL ENTRY";
                            else if (stValue[0] == 'M')
                                transInfo += "MAGSTRIPE";
                            else if (stValue[0] == 'F')
                                transInfo += "FALLBACK";
                            else if (stValue[0] == 'C')
                                transInfo += "CHIP";
                            else if (stValue[0] == 'P')
                                transInfo += "CONTACTLESS";
                            else
                                transInfo += stValue;
                            approvedResponse.EntryMode = stValue;
                            transInfo += "\r\n";
                        }

                        if (GetTagValue(response, 53, out stValue) == 0)
                        {

                            var appLabel = stValue.Substring(30, 16);
                            transInfo += "APP LABEL: ";
                            transInfo += appLabel;
                            transInfo += "\r\n";
                            approvedResponse.AppLabel = appLabel;

                            var aid = stValue.Substring(14, 16);
                            transInfo += "AID: ";
                            transInfo += aid;
                            transInfo += "\r\n";
                            approvedResponse.Aid = aid;

                            
                            transInfo += "TVR TSI: ";
                            transInfo += stValue.Substring(0, 10);
                            transInfo += " ";
                            transInfo += stValue.Substring(10, 4);
                            transInfo += "\r\n";

                            var tc = stValue.Substring(46, 16);
                            transInfo += "TC: ";
                            transInfo += stValue.Substring(46, 16);
                            transInfo += "\r\n";
                            approvedResponse.Tc = tc;

                        }

                        if (GetTagValue(response, 4, out stValue) == 0)
                        {
                            transInfo += "AMOUNT:   ";
                            var stAmount = int.Parse(stValue.Substring(0, 10)).ToString();
                            transInfo += "$";
                            transInfo += stAmount;
                            transInfo += ".";
                            transInfo += stValue.Substring(10, 2);
                            decimal.TryParse(stAmount + "." + stValue.Substring(10, 2), out decimal amt);
                            approvedResponse.Amount = amt;
                        }

                        transInfo += "\r\n";
                        
                        LogIucApi(transInfo);
                        OnSaleApproved?.Invoke(this,approvedResponse);
                        #endregion
                    }
                    else if (string.CompareOrdinal(responseCode, "CT") == 0 || string.CompareOrdinal(responseCode, "TA") == 0)
                    {
                        Task.Run(() =>
                        {
                            if (!cancelTransactionSilently)
                            {
                                OnSaleCancelled?.Invoke(this, saleReponse);
                            }
                            cancelTransactionSilently = false;

                            LogIucApi($"SALE CANCELLED: {JsonConvert.SerializeObject(saleReponse)}");
                        });

                    }
                    else
                    {
                        Task.Run(() =>
                        {
                            OnSaleError?.Invoke(this,saleReponse);
                            LogIucApi($"SALE ERROR: {JsonConvert.SerializeObject(saleReponse)}");

                        });
                    }



                }

            }
            else if (String.Compare(response, 0, "R900", 0, 3) == 0)
            {
                var stTerminalInfoValue = response.Substring(4);
                stTerminalInfoValue += "\r\n";
                LogIucApi($"Terminal Response: {stTerminalInfoValue}");
            }
        }

        #endregion

        #region Utils

        private void ClearResponseQueue()
        {
            this._responseQueue = new ConcurrentQueue<string>();
        }
        private void QueueResponse(string response)
        {
            // Never expect to get large queue
            if (this._responseQueue.Count > 30)
                this.ClearResponseQueue();
            this._responseQueue.Enqueue(response);
        }

        private string GetResponse()
        {
            return this._responseQueue.TryDequeue(out string result) ? result : "";
        }

        private string GetExpectedResponse(string startWith)
        {
            var tryTime = 0;
            while (true)
            {
                Sleep(1);
                if (++tryTime > 15000)
                {
                    return string.Empty;
                }
                var currentReponse = GetResponse();
                if (currentReponse.StartsWith(startWith))
                {
                    return currentReponse;
                }
            }
        }

        public static int GetTagValue(string responseString, int inTag, out string outValue)
        {
            var i = 0;
            if (responseString == null)
                goto FnEnd;
            for (i = 4; i < responseString.Length;)
            {
                var tag = responseString.Substring(i, 2);
                i += 2;
                var length = responseString.Substring(i, 2);
                i += 2;
                if (int.Parse(length) > 0)
                {
                    var value = responseString.Substring(i, int.Parse(length));

                    if (int.Parse(tag) == inTag)
                    {
                        outValue = value;
                        return 0;
                    }
                    i += int.Parse(length);
                }
            }

            FnEnd:
            outValue = null;
            return 1; // Error tag not found
        }


        #endregion

        #region Commands

        public void SendSaleCommand(string command, Action<IucApprovedResponse> onSaleApproved = null, Action<SaleResponse> onSaleError = null, Action<SaleResponse> onSaleCancelled = null, Action<string> onTerminalCallback = null)
        {
            lastSaleDateTime = DateTime.Now;
            SendCommand(command);
        }

        public bool CheckDevice()
        {
            var result = false;
            var response = PollingDevice();
            if (!string.IsNullOrEmpty(response) && response.StartsWith("R902"))
            {
                result = true;
            }
            return result;
        }

        public Dictionary<string, string> TerminalInfo()
        {
            var response = SendAndRecv("C900", "R900");
            LogIucApi("Terminal Info reponse: " + response);

            var infos = response.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var title = infos.FirstOrDefault(x => x.Contains("R900"))?.Replace("R900", String.Empty).Trim();
            var version = infos.FirstOrDefault(x => x.Contains("VERSION"))?.Replace("VERSION", String.Empty).Trim();
            var tid = infos.FirstOrDefault(x => x.Contains("MMS APP: DBS   MMS TID:"))?.Replace("MMS APP: DBS   MMS TID:", String.Empty).Trim();
            var autoSett = infos.FirstOrDefault(x => x.Contains("AUTO SETTLE (HHMM):"))?.Replace("AUTO SETTLE (HHMM):", String.Empty).Trim();
            var localIp = infos.FirstOrDefault(x => x.Contains("LOCAL ADDR:"))?.Replace("LOCAL ADDR:", String.Empty).Trim();

            var dict = new Dictionary<string, string>
            {
                { "TITLE", title },
                { "VERSION", version },
                { "TID", tid },
                { "AUTOSETT", autoSett },
                { "IP", localIp }
            };
            return dict;
        }

        public bool CheckBlacklist()
        {
            var response = SendAndRecv("C616", "R616");
            LogIucApi("Check Blacklist reponse: " + response);
            if (GetTagValue(response, 39, out string responseCode) == 0)
            {
                if (responseCode == "BL")
                {
                    return true;
                }
            }
            return false;
        }



        public string PollingDevice()
        {
            LogIucApi($"PollingDevice {DateTime.Now} - Last sale {lastSaleDateTime?.ToString()}");
            string response = string.Empty;
            //default timeout is 90 seconds
            //if last payment time within 120 seconds, no need to polling
            if (lastSaleDateTime.HasValue && ((DateTime.Now - lastSaleDateTime.Value).TotalSeconds <= 120))
            {
                LogIucApi("Sale just started, no need polling");
                response = "R902";
            }
            else
            {
                response = SendAndRecv("C902", "R902");
            }
            // notify to Main about device state
            if (!string.IsNullOrEmpty(response) && response.StartsWith("R902"))
            {

                IsTerminalRunning = true;

            }
            else
            {
                IsTerminalRunning = false;
                
            }

            LogIucApi($"PollingDevice result {response}");
            return response;

        }

        public void CancelCommand()
        {
            WritePortData(new byte[] { 0x18 });
        }
        public void Enq()
        {
            WritePortData(new byte[] { 0x05 });
        }
        public void Eot()
        {
            WritePortData(new byte[] { 0x04 });
        }
        public void Ack()
        {
            WritePortData(new byte[] { 0x06 });
        }
        public void NonAck()
        {
            WritePortData(new byte[] { 0x15 });
        }

        public void SendCommand(string command)
        {
            this._cmdBuilder = new StringBuilder();
            var cmdByte = command.AsiiToBytes().ToList();
            cmdByte.Add(0x03);
            var lrc = cmdByte.ToArray().CheckSum();
            cmdByte.Add(lrc);
            cmdByte.Insert(0, 0x02);

            this.Enq();
            WritePortData(cmdByte.ToArray());
            this.Eot();

            
        }
        public void SendCommandJob()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var hasData = this._commandQueue.TryDequeue(out string command);
                    if (hasData)
                    {
                        this._cmdBuilder = new StringBuilder();
                        var cmdByte = command.AsiiToBytes().ToList();
                        cmdByte.Add(0x03);
                        var lrc = cmdByte.ToArray().CheckSum();
                        cmdByte.Add(lrc);
                        cmdByte.Insert(0, 0x02);

                        this.Enq();
                        WritePortData(cmdByte.ToArray());
                        this.Eot();


                    //var tryTime = 0;
                    //CMD_EQN:
                    //// Enquire and waiting for ACK
                    //var eqn = Enquire();
                    //if (!eqn)
                    //{
                    //    if (++tryTime > 3)
                    //    {
                    //        // Fail to eqn
                    //        continue;
                    //    }
                    //    // Try to resend eqn
                    //    goto CMD_EQN;
                    //}
                    //tryTime = 0;

                    //CMD_SEND:
                    //WritePortData(cmdByte.ToArray());
                    //if (WaitingForAck())
                    //{
                    //    Eot();
                    //}
                    //else
                    //{
                    //    if (++tryTime > 3)
                    //    {
                    //        // Fail to send
                    //        continue;
                    //    }
                    //    // Try to resend
                    //    goto CMD_SEND;
                    //}
                }
                }
            });

        }

        public bool Enquire()
        {
            this.ClearResponseQueue();
            Enq();
            return WaitingForAck();
        }

        public bool WaitingForAck()
        {
            return GetExpectedResponse("06") == "06";
        }

        public string SendAndRecv(string command, string expectedResponse = null)
        {
            this.ClearResponseQueue();
            Debug?.Invoke($"SendAndRecv ----> {command} | Expect reponse {Convert.ToString(expectedResponse)}");
            SendCommand(command);
            if (expectedResponse == null)
            {
                expectedResponse = command.Take(4).ToString().Replace("C", "R");
            }
            var response = GetExpectedResponse(expectedResponse);
            Debug?.Invoke("SendAndRecv <---- " + response);

            return response;
        }

        public void Dispose()
        {
            if (Port != null)
            {
                if (Port.IsOpen)
                    Port.Close();
                Port.Dispose();
            }
        }

        public void LogIucApi(string message)
        {
            if (_logger != null)
                _logger.LogIucApi($"[{ComportName}]: {message}");
        }

        public bool EnablePayment(int cents)
        {
            if (!Port.IsOpen)
            {
                LogIucApi("Serial port is not open. Coudln't start sale");
                return false;
            }

            if (Mode == IucPaymentMode.CEPAS)
            {
                LogIucApi("Start Sale mode: CEPAS");

                SendSaleCommand(Commands.Cpas(cents));
            }
            else
            {
                LogIucApi("Start Sale mode: CONTACTLESS");
                SendSaleCommand(Commands.Contactless(cents));
            }
            LogIucApi($"Price: {cents}c");
            return true;
        }
        /// <summary>
        /// Cancel current transaction.
        /// </summary>
        /// <param name="silent"> will not postback Cancelled Sale </param>
        /// <returns></returns>
        public bool DisablePayment(bool silent = false)
        {
            cancelTransactionSilently = silent;
            CancelCommand();
            return true;
        }

        public class Commands
        {
            private static int saleTimeout = 0;
            static Commands()
            {
                saleTimeout = 60;//60 seconds, Ha hardcode for default value
            }

            public static int CmdIdentity = 0;

            public static string Sale(int amount)
            {

                var tags = new List<CommandTag>()
                {
                    new CommandTag(4,12,amount),
                    new CommandTag(1,3,saleTimeout),
                    new CommandTag(57,06,InreaseIdentity())
                };
                var cmd = new CommandInfomation("C200", "R200", tags);
                return cmd.ToString();
            }
            public static string Contactless(int amount)
            {
                var tags = new List<CommandTag>()
                {
                    new CommandTag(4,12,amount),
                    new CommandTag(1,3,saleTimeout),
                    new CommandTag(57,06,InreaseIdentity())
                };
                var cmd = new CommandInfomation("C630", "R630", tags);
                return cmd.ToString();
            }

            public static string Cpas(int amount)
            {
                var tags = new List<CommandTag>()
                {
                    new CommandTag(4,12,amount),
                    new CommandTag(1,3,60),
                    new CommandTag(57,06,InreaseIdentity())
                };
                var cmd = new CommandInfomation("C610", "R610", tags);
                return cmd.ToString();
            }

            public static string Void()
            {
                var tags = new List<CommandTag>()
                {
                    new CommandTag(57,06,InreaseIdentity())
                };
                var cmd = new CommandInfomation("C300", "R300", tags);
                return cmd.ToString();
            }

            public static string Settlement()
            {
                var tags = new List<CommandTag>()
                {
                    new CommandTag(57,06,InreaseIdentity())
                };
                var cmd = new CommandInfomation("C700", "R700", tags);
                return cmd.ToString();
            }

            public static int InreaseIdentity()
            {
                CmdIdentity++;
                if (CmdIdentity == 999999)
                    CmdIdentity = 0;
                return CmdIdentity;
            }
            public class CommandInfomation
            {
                public CommandInfomation(string command, string responseCommand, List<CommandTag> tags)
                {
                    Command = command;
                    ResponseCommand = responseCommand;
                    Tags = tags;
                }
                public string Command { get; set; }
                public string ResponseCommand { get; set; }
                public List<CommandTag> Tags { get; set; }

                public override string ToString()
                {
                    var tags = string.Empty;
                    if (Tags != null && Tags.Count > 0)
                    {
                        Tags.ForEach(x => tags += x.ToString());
                    }
                    return $"{Command}{tags}";
                }
            }

            public class CommandTag
            {
                public CommandTag(int tag, int length, object value)
                {
                    Tag = tag;
                    Length = length;
                    Value = value;
                }
                public int Tag { get; set; }
                public int Length { get; set; }
                public object Value { get; set; }

                public override string ToString()
                {
                    var tag = Tag.ToString().PadLeft(2, '0');
                    var length = Length.ToString().PadLeft(2, '0');
                    var value = Value?.ToString().PadLeft(Length, '0');

                    return $"{tag}{length}{value}";
                }
            }
        }
        #endregion
    }


    #region EXT
    public enum IucPaymentMode
    {
        CEPAS,
        CONTACTLESS
    }
    public static class Ext
    {



        public static int ByteToInt(this byte data)
        {
            return Convert.ToInt32(data);
        }
        public static byte CheckSum(this byte[] data)
        {
            byte crc = 0;
            for (int i = 0; i < data.Length; ++i)
            {
                crc = (byte)(crc ^ data[i]);
            }
            return crc;
        }

        public static string TryGetValue(this Dictionary<string, string> dict, string key)
        {
            if (dict.TryGetValue(key, out string value))
            {
                return value.HexStringToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public static byte[] StringToByteArray(this String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2) bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static byte[] CmdToByteArray(this String cmd)
        {
            int NumberChars = cmd.Length;
            byte[] bytes = new byte[NumberChars];
            for (int i = 0; i < NumberChars; i += 1) bytes[i] = Convert.ToByte(cmd.Substring(i, 1), 16);
            return bytes;
        }

        public static string HexStringToString(this String hex)
        {
            return Encoding.ASCII.GetString(hex.StringToByteArray());
        }


        public static string ToHexString(this byte[] hex)
        {
            if (hex == null) return null;
            if (hex.Length == 0) return string.Empty;

            var s = new StringBuilder();
            foreach (byte b in hex)
            {
                s.Append(b.ToString("x2").ToUpper());
                s.Append(" ");
            }
            return s.ToString();
        }


        public static string ToHexStringNoSpace(this byte[] hex)
        {
            if (hex == null) return null;
            if (hex.Length == 0) return string.Empty;

            var s = new StringBuilder();
            foreach (byte b in hex)
            {
                s.Append(b.ToString("x2").ToUpper());
            }
            return s.ToString();
        }

        public static string ToAsiiString(this byte[] hex)
        {
            return Encoding.UTF8.GetString(hex, 0, hex.Length);
        }
        public static byte[] AsiiToBytes(this String data)
        {
            return ASCIIEncoding.ASCII.GetBytes(data);
        }

        public static byte[] ToHexBytes(this string hex)
        {
            if (hex == null) return null;
            if (hex.Length == 0) return new byte[0];

            int l = hex.Length / 2;
            var b = new byte[l];
            for (int i = 0; i < l; ++i)
            {
                var hexs = hex.Substring(i * 2, 2);
                b[i] = Convert.ToByte(hexs, 16);
            }
            return b;
        }

        public static byte[] IntToBcd(this int number)
        {
            return number.ToString().PadLeft(4, '0').StringToByteArray();
        }
    }
    #endregion

}
