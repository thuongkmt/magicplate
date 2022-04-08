using KonbiBrain.Common.Services;
using Konbini.Messages.Commands.RFIDTable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Konbi.Simulator
{
    public partial class TransactionStressTest : Form,INotifyPropertyChanged
    {
        private readonly LogService logger = new LogService();
        private readonly NsqMessageProducerService nsqService;
        private bool isRunning;
        public bool IsRunning { get { return isRunning; }
            set {
                if (isRunning != value)
                {
                    isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
                    

                }
            } }
        private MdbCashless MdbCashless;
        public event PropertyChangedEventHandler PropertyChanged;

        protected List<Plate> Plates { get; set; }
        protected CancellationTokenSource TokenSource { get; set; }
        protected string ConfigFile => Path.Combine(Environment.CurrentDirectory, "transactionStressTest.txt");
        public TransactionStressTest()
        {
            InitializeComponent();
            TokenSource = new CancellationTokenSource();
            nsqService = new NsqMessageProducerService();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // save to file
            TokenSource.Cancel();
            TokenSource = new CancellationTokenSource();
            
            Task.Run(async () => {
                try
                {
                    await WriteConfigFileContentAsync(TokenSource.Token);
                }
                catch (OperationCanceledException)
                {

                    
                }
                
            },TokenSource.Token);
            

        }

        private void TransactionStressTest_Load(object sender, EventArgs e)
        {
            // read config from file
            txtPlateConfig.Text = ReadConfigFileContent();
            GetPlates(txtPlateConfig.Text);
            this.txtPlateConfig.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.PropertyChanged += TransactionStressTest_PropertyChanged;

        }

        private void TransactionStressTest_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsRunning":
                    {
                        var btnText = IsRunning ? "Stop" : "Start";
                        if (btnStart.InvokeRequired)
                            btnStart.Invoke((Action)(() => { btnStart.Text = btnText; }));
                        else
                            btnStart.Text = btnText;

                        if (txtPlateConfig.InvokeRequired)
                            txtPlateConfig.Invoke((Action)(() => { txtPlateConfig.Enabled = !IsRunning; }));
                        else
                            txtPlateConfig.Enabled = !IsRunning;
                       
                    }
                    break;
                default:
                    break;  
            }
        }

        private string ReadConfigFileContent()
        {
            if (!File.Exists(ConfigFile))
                return string.Empty;
            return File.ReadAllText(ConfigFile);
        }
        private async Task WriteConfigFileContentAsync(CancellationToken token)
        {
             await Task.Delay(1000, token);
            if (!token.IsCancellationRequested)
            {
                GetPlates(txtPlateConfig.Text);
                File.WriteAllText(ConfigFile, txtPlateConfig.Text.Trim());
            }
                
        }
        private void GetPlates(string content)
        {
            var parsingArray =  content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Plates= parsingArray.Select(el => el.Split('-')).Where(el => el.Length == 2).Select(el => new Plate { Type = el[0], Uid = el[1] }).ToList();
            if (lblPlateCount.InvokeRequired)
            {
                lblPlateCount.Invoke((Action)(()=>{ lblPlateCount.Text = Plates.Count.ToString(); }));
            }
            else
            {
                lblPlateCount.Text = Plates.Count.ToString();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if(MdbCashless==null || MdbCashless.IsDisposed)
            {
                MdbCashless = new MdbCashless();
            }
            TokenSource.Cancel();
            TokenSource = new CancellationTokenSource();
            IsRunning = !IsRunning;
            if (IsRunning)
                if (ValidateInput())

                    RunAsync(TokenSource.Token).ContinueWith(taskResult => {
                        if(taskResult.Exception!=null)
                            logger.LogException(taskResult.Exception);
                        MessageBox.Show("Done");
                        IsRunning = false;
                    });
                else
                {
                    IsRunning = false;
                }
            else
            {

            }

        }

        private bool ValidateInput()
        {
            if (Plates==null ||Plates.Count <= 0)
            {
                MessageBox.Show("Invalid plates. please enter couple plates to start testing");
                return false;
            }
            return true;
        }
        int startIndex = 0;
        private async Task RunAsync(CancellationToken token)
        {
            startIndex = 0;
            int.TryParse(txtTranNumber.Text, out int tranNumber);
            while (!token.IsCancellationRequested && startIndex<tranNumber)
            {
                startIndex++;
                await Task.Delay(1000,token);
                var command = new DetectPlatesCommand();
                command.CommandObject = new DetectPlatesCommandPayload();
                command.CommandObject.Plates = GetScanningPlates(Plates).OrderBy(el => el.Type).ThenBy(el => el.Uid).Select(el => new PlateInfo { UType = el.Type, UID = el.Uid });
                nsqService.SendRfidTableResponseCommand(command);
                await Task.Delay(8000, token);
                var paymentState = true;
                if (cbRandomPayment.Checked)
                {
                    var isPaymentSuccess = DateTime.Now.Second % 2 == 0;
                    if (isPaymentSuccess)
                    {
                        // success
                        MdbCashless.button1_Click(MdbCashless, null);
                       
                    }
                    else
                    {
                        //error
                        MdbCashless.button2_Click(MdbCashless, null);
                        paymentState = false;
                    }
                   
                    
                }
                else
                {
                    // success
                    MdbCashless.button1_Click(MdbCashless, null);
                }
                logger.LogRawInputInfo($"Scanning: {command.CommandObject.Plates.Count()}: {string.Join(" | ", command.CommandObject.Plates.ToList().Select(el => $"{el.UType}-{el.UID}")) } Payment State: {paymentState}");
                lblPlateCount.Invoke((Action)(()=>{ lblPlateCount.Text = command.CommandObject.Plates.Count().ToString(); }));
                lblScanning.Invoke((Action)(() => { lblScanning.Text = startIndex.ToString(); }));
                // wait a moment
                await Task.Delay(2000, token);
                command = new DetectPlatesCommand();
                command.CommandObject = new DetectPlatesCommandPayload();
                command.CommandObject.Plates = new List<PlateInfo>();
                nsqService.SendRfidTableResponseCommand(command);
                //await Task.Delay(2000, token);





            }
          
        }
        private IList<Plate> GetScanningPlates(IList<Plate> plates)
        {
            var randInt = new Random();
            var take = randInt.Next(0, plates.Count+1);
            if (take == 0) take = 1;
            if (take > plates.Count) take = plates.Count;
            return plates.OrderBy(el => randInt.Next(100)).Take(take).ToList();
        }
    }
    public class Plate
    {
        public string Type { get; set; }
        public string Uid { get; set; }
    }
}
