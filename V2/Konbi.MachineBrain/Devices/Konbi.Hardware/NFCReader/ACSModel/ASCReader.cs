using PCSC;
using PCSC.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PCSC.Iso7816;
using PCSC.Exceptions;

namespace Konbi.Hardware.NFCReader.ACSModel
{
    public class ASCReader : IASCReader
    {
        private SCardMonitor _monitor;
        private readonly IContextFactory _contextFactory = ContextFactory.Instance;
        private SCardMonitor monitor;
        private System.Timers.Timer _reconnectTimer;
        private bool _autoReconnect = true;
        private bool _isConnected = false;
        /// <summary>
        /// current detected card number
        /// </summary>
        public string NFCUID { get; set; }
        /// <summary>
        /// Raised when card inserted with card number returned
        /// </summary>
        public Action<string> OnInsertedCard { get; set; }
        /// <summary>
        /// Raised when card removed
        /// </summary>
        public Action OnRemovedCard { get; set; }
        /// <summary>
        /// Raised right before scanning card for data
        /// </summary>
        public Action OnScanningCard { get; set; }
        /// <summary>
        /// Raised when finishing reading card
        /// </summary>
        public Action OnScannedCard { get; set; }
        /// <summary>
        /// when having error
        /// </summary>
        public Action OnError { get; set; }
        /// <summary>
        /// when reader is connected/disconnected. return is boolean
        /// </summary>
        public Action<bool> OnConnectedReader { get; set; }

        public string[] GetReaders()
        {
            try
            {
                using (var context = _contextFactory.Establish(SCardScope.System))
                {
                    return context.GetReaders();
                }
            }
            catch (Exception ex)
            {                
                //TODO: using injected Logger for logging?
                return null;
            }
        }
        public bool Init()
        {
            try
            {
                if (!_autoReconnect)
                    return true;
             
                monitor = null;

                StartNFCMonitor();

                return !_autoReconnect;
            }
            catch (Exception ex)
            {
                //TODO: using injected Logger for logging?
                return false;
            }
        }
        private void StartNFCMonitor()
        {
            try
            {
                if (monitor != null)
                    return;

                var readerName = GetReaders();

                if (readerName.Count() == 0)
                {
                    if(_isConnected)
                        OnConnectedReader?.Invoke(false);

                    _autoReconnect = true;
                    _isConnected = false;
                    StartReconnectTimer();
                    return;
                }
                else
                {
                    monitor = new SCardMonitor(_contextFactory, SCardScope.System);

                    AttachToEvents(monitor);
                    
                    monitor.Start(readerName);

                    if (_autoReconnect)
                        _autoReconnect = false;

                    if(!_isConnected)
                        OnConnectedReader?.Invoke(true);
                    _isConnected = true;
                }
            }
            catch (Exception ex)
            {
                //TODO: using injected Logger for logging?
                
            }
        }
        private void StartReconnectTimer()
        {
            _reconnectTimer = new System.Timers.Timer();
            _reconnectTimer.Interval = 1000; 
            _reconnectTimer.Elapsed += _reconnectTimer_Elapsed;
            _reconnectTimer.AutoReset = false;
            _reconnectTimer.Start();
        }

        private void _reconnectTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Init())
            {
                _reconnectTimer.Stop();
            }
            else
            {
                _reconnectTimer.Enabled = true;
            }
        }

        private void AttachToEvents(ISCardMonitor monitor)
        {
            monitor.CardInserted += (sender, args) => HandleEvent("CardInserted", args);
            monitor.CardRemoved += (sender, args) => HandleEvent("CardRemoved", args);
            //monitor.Initialized += (sender, args) => DisplayEvent("Initialized", args);
            //monitor.StatusChanged += StatusChanged;
            monitor.MonitorException += (sender, args) => MonitorException(sender, args);
        }

        private void HandleEvent(string eventName, CardStatusEventArgs unknown)
        {

            switch (eventName)
            {
                case "CardInserted":
                    NFCUID = GetCardUID(unknown.ReaderName);
                    OnInsertedCard?.Invoke(NFCUID);
                    break;
                case "CardRemoved":
                    NFCUID = "";
                    OnRemovedCard?.Invoke();
                    break;
            }
        }

        private void MonitorException(object s, PCSCException a)
        {
            //LogService?.LogException(a.Message);
            StartReconnectTimer();
            _autoReconnect = true;
            OnError?.Invoke();

            //LogService.SendToSlackAlert("Cannot connect to NFC Reader.");
        }

        private string GetCardUID(string readerName)
        {
            OnScanningCard?.Invoke();

            var returnStr = "";

            try
            {
                var contextFactory = ContextFactory.Instance;

                using (var ctx = contextFactory.Establish(SCardScope.System))
                {
                    using (var rfidReader = new SCardReader(ctx))
                    {
                        var sc = rfidReader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);

                        if (sc == SCardError.Success)
                        {
                            var apdu = new CommandApdu(IsoCase.Case2Short, rfidReader.ActiveProtocol)
                            {
                                CLA = 0xFF, // Class
                                Instruction = InstructionCode.GetData,
                                P1 = 0x00, // Parameter 1
                                P2 = 0x00, // Parameter 2
                                Le = 0x00 // Expected length of the returned data
                            };

                            try
                            {
                                var receivePci = new SCardPCI(); // IO returned protocol control information.
                                var sendPci = SCardPCI.GetPci(rfidReader.ActiveProtocol);

                                var receiveBuffer = new byte[256];
                                var command = apdu.ToArray();
                                var data = new byte[256];

                                sc = rfidReader.Transmit(
                                    sendPci, // Protocol Control Information (T0, T1 or Raw)
                                    command, // command APDU
                                    receivePci, // returning Protocol Control Information
                                    ref receiveBuffer); // data buffer

                                if (sc != SCardError.Success)
                                {
                                    //LogService.LogInfo("NFCReader::Could not transmit to reader.");
                                    //rfidReader.EndTransaction(SCardReaderDisposition.Leave);
                                    //return returnStr;
                                }
                                else
                                {
                                    var responseApdu = new ResponseApdu(receiveBuffer, IsoCase.Case2Short, rfidReader.ActiveProtocol);
                                    if (responseApdu.HasData)
                                    {
                                        receiveBuffer.CopyTo(data, 0);
                                        //returnStr = BitConverter.ToInt64(data, 0).ToString();
                                        var cardData = data.Take(4).ToArray();
                                        var uid = BitConverter.ToString(cardData);
                                        returnStr = uid;
                                    }
                                }
                                //}

                            }
                            catch (Exception ex)
                            {
                                //LogService.LogException(ex);
                            }
                            //finally
                            //{
                            //    //rfidReader.EndTransaction(SCardReaderDisposition.Leave);
                            //    //rfidReader.Disconnect(SCardReaderDisposition.Eject);
                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: using injected Logger for logging?
            }

            OnScannedCard?.Invoke();

            return returnStr;
        }
    }
    
}
