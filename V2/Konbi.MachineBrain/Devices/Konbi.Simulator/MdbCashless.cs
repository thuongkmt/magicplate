using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KonbiBrain.Common.Messages;
using KonbiBrain.Common.Messages.Payment;
using KonbiBrain.Common.Services;
using KonbiBrain.Messages;
using Konbini.Messages.Enums;
using Newtonsoft.Json;
using NsqSharp;

namespace Konbi.Simulator
{
    public partial class MdbCashless : Form, INsqHandler
    {
        private readonly string paymentType = PaymentTypes.Mdb_CASHLESS.ToString();
        private readonly NsqMessageProducerService nsqProducerService;
        private readonly NsqMessageConsumerService nsqConsumerService;
        private decimal price;
        private readonly Consumer consumer;

        public MdbCashless()
        {
            InitializeComponent();
            nsqProducerService = new NsqMessageProducerService();            
            consumer = new Consumer(NsqTopics.PAYMENT_REQUEST_TOPIC,"SimulatorChannel");
            consumer.AddHandler(this);
            // use ConnectToNsqd instead of using ConnectToNsqLookupd because  we use standalone nsq service not cluster one.
            consumer.ConnectToNsqd(NsqConstants.NsqUrlConsumer);

        }

        private void BtnUserScan_Click(object sender, EventArgs e)
        {
            Guid txtGuid;
            Guid.TryParse(TxtTranCode.Text.Trim(), out txtGuid);
            SimulateEnablePaymentAsync(true, txtGuid);           
        }

        private void MdbCashless_FormClosed(object sender, FormClosedEventArgs e)
        {
            nsqProducerService?.Dispose();
        }

        public void button1_Click(object sender, EventArgs e)
        {
            var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
            {
                Response = new PaymentResponseData()
                {
                    Message = "Payment success",
                    State = Konbini.Messages.Enums.PaymentState.Success,
                    ResponseObject = new
                    {
                        Tid = "60002643",
                        Mid = "600054000000085",
                        DateTime = "07/03 13:05:07",
                        Invoice = "000138",
                        Batch = "000003",
                        CardLabel = "EZLINK",
                        CardNumber = "8008160001246183",
                        Rrn = "0307130507",
                        ApproveCode = "000001",
                        Amount = price
                    },
                    OtherInfo = null
                }
            };
            nsqProducerService.SendPaymentResponseCommand(cmd);
        }

        public void button2_Click(object sender, EventArgs e)
        {
            var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
            {
                Response = new PaymentResponseData()
                {
                    Message = "Payment Error",
                    State = Konbini.Messages.Enums.PaymentState.Failure,
                    ResponseObject = null,
                    OtherInfo = null
                }
            };
            nsqProducerService.SendPaymentResponseCommand(cmd);
        }

        private void MdbCashless_Load(object sender, EventArgs e)
        {
            
        }

        public void HandleMessage(IMessage message)
        {
            var msg = Encoding.UTF8.GetString(message.Body);
            var obj = JsonConvert.DeserializeObject<NsqPaymentCommandBase>(msg);
            if (obj == null)// || obj.PaymentType != paymentType)
                return;

            // Complete Received 
            CompleteReceivedAsync(obj).Wait();
            if (obj.Command == UniversalCommandConstants.EnablePaymentCommand)
            {
                var cmd = JsonConvert.DeserializeObject<NsqEnablePaymentCommand>(msg);
                this.price = cmd.Amount * 100;
                //Activate device 
                if (ckbSimulateEnablePayment.Checked)
                {
                    SimulateEnablePaymentAsync(rbtnEnablePaymentSuccess.Checked, obj.TransactionId);


                }

            }
        }
        private async Task SimulateEnablePaymentAsync(bool success,Guid transactionId)
        {
            var command = new NsqEnablePaymentResponseCommand(paymentType);

            command.TransactionId = transactionId;
            command.Code = success ? 0 : 1;
            nsqProducerService.SendPaymentResponseCommand(command);

            await Task.Delay(300);
            if (success)
            {
                var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
                {
                    Response = new PaymentResponseData()
                    {
                        Message = "PLEASE TAP CARD",
                        State = Konbini.Messages.Enums.PaymentState.InProgress,
                        ResponseObject = null,
                        OtherInfo = null
                    }
                };
                nsqProducerService.SendPaymentResponseCommand(cmd);

            }



        }

        private async Task CompleteReceivedAsync(NsqPaymentCommandBase obj)
        {
            await Task.Delay(10);
            var command = new NsqPaymentACKResponseCommand(obj.CommandId, obj.PaymentType);
            nsqProducerService.SendPaymentResponseCommand(command);
        }

        public void LogFailedMessage(IMessage message)
        {
            
        }

        private void btnPaymentInProgress_Click(object sender, EventArgs e)
        {
            
                var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
                {
                    Response = new PaymentResponseData()
                    {
                        Message = "Please Wait",
                        State = Konbini.Messages.Enums.PaymentState.InProgress,
                        ResponseObject = null,
                        OtherInfo = null
                    }
                };
                nsqProducerService.SendPaymentResponseCommand(cmd);

           
        }
    }
}
