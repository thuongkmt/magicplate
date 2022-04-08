using KonbiBrain.Common.Messages;
using KonbiBrain.Common.Messages.Payment;
using KonbiBrain.Common.Services;
using KonbiBrain.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Payment;
using Newtonsoft.Json;
using NsqSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Konbi.Simulator
{
    public partial class Payment : Form, INsqHandler
    {
        private readonly NsqMessageProducerService nsqService;
        private NsqMessageConsumerService _nsqConsumer;
        private readonly string paymentType = PaymentTypes.KONBI_CREDITS.ToString();
        private bool isEnabled = false;
        private decimal requestedAmount = 0;
        public decimal RequestedAmount
        {
            get { return requestedAmount; }
            set
            {
                requestedAmount = value;
                if (lblAmount.InvokeRequired)
                {
                    lblAmount.BeginInvoke((MethodInvoker)delegate () { lblAmount.Text = RequestedAmount.ToString("C"); });
                }
                else
                {
                    lblAmount.Text = RequestedAmount.ToString("C");
                }
            }
        }
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                isEnabled = value;
                if (!isEnabled)
                {

                    RequestedAmount = 0;
                    //btnApprove.Enabled = false;
                    //btnReject.Enabled = false;
                }
                else
                {
                    //btnApprove.Enabled = true;
                    //btnReject.Enabled = true;
                }
            }
        }
        public Payment()
        {
            InitializeComponent();
            nsqService = new NsqMessageProducerService();
            _nsqConsumer = new NsqMessageConsumerService(topic: NsqTopics.PAYMENT_REQUEST_TOPIC, handler: this, ClientId: "KonbiCreditsPayment");
            
        }

        public void HandleMessage(IMessage message)
        {
            var msg = Encoding.UTF8.GetString(message.Body);
            var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);

            if (obj.IsTimeout())
            {
                return;
            }
            if (obj.Command == UniversalCommandConstants.EnablePaymentCommand)
            {
                var cmd = JsonConvert.DeserializeObject<NsqEnablePaymentCommand>(msg);


                if (obj.Command == UniversalCommandConstants.EnablePaymentCommand)
                {

                    //Activate device 
                    //int priceInCents = (int)(cmd.Amount * 100);
                    RequestedAmount = cmd.Amount;
                    var isActivated = false;
                    if (Enum.TryParse<PaymentTypes>(cmd.PaymentType, out PaymentTypes pType))
                    {
                        if (pType == PaymentTypes.KONBI_CREDITS)
                        {
                            CompleteReceivedAsync(cmd).Wait();

                            var command = new NsqEnablePaymentResponseCommand(cmd.PaymentType);
                            command.TransactionId = cmd.TransactionId;
                            isActivated = true;
                            command.Code = isActivated ? 0 : 1;
                            var isSent = nsqService.SendPaymentResponseCommand(command);
                            IsEnabled = true;
                        }                      
                    }

                }
            }
            else if (obj.Command == UniversalCommandConstants.DisablePaymentCommand)
            {
                var cmd = JsonConvert.DeserializeObject<NsqDisablePaymentCommand>(msg);
                if (Enum.TryParse<PaymentTypes>(cmd.PaymentType, out PaymentTypes pType))
                {
                    if (pType == PaymentTypes.KONBI_CREDITS)
                    {
                        CompleteReceivedAsync(cmd).Wait();
                        IsEnabled = false;
                    }
                }
                   
              
            }
        }
        private async Task CompleteReceivedAsync(NsqPaymentCommandBase obj)
        {
            await Task.Delay(10);
            var command = new NsqPaymentACKResponseCommand(obj.CommandId, obj.PaymentType);
            nsqService.SendPaymentResponseCommand(command);
        }

        public void LogFailedMessage(IMessage message)
        {
            //throw new NotImplementedException();
        }

        private void btnApprove_Click(object sender, EventArgs e)
        {
            var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
            {
                Response = new PaymentResponseData()
                {
                    Message = "Payment approved",
                    State =  PaymentState.Success,
                    //ResponseObject = response,
                    OtherInfo = null
                }
            };
            nsqService.SendPaymentResponseCommand(cmd);
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
            {
                Response = new PaymentResponseData()
                {
                    Message = "Cannot validate your membership info",
                    State = PaymentState.Failure,
                    //ResponseObject = response,
                    OtherInfo = null
                }
            };
            nsqService.SendPaymentResponseCommand(cmd);
        }
    }
}
