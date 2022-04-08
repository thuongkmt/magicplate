using Konbini.Messages;
using MessagePack;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Konbini.Messages.Enums;

namespace Konbi.Simulator
{
    public partial class FormMain : Form
    {
        private RFIDTable tableForm;
        public MdbCashless cashlessForm;
        private MQTTClientTest mQTTClientTest;
        private TransactionStressTest transactionStressTest;
        private AlarmLight alarmLightForm;
        private Payment paymentForm;

        public FormMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (tableForm == null|| tableForm.IsDisposed)
            {
                tableForm = new RFIDTable();
                tableForm.Show();

            }
            tableForm.Activate();


        }

        private void BtnMdbCashless_Click(object sender, EventArgs e)
        {
            if (cashlessForm == null|| cashlessForm.IsDisposed)
            {
                cashlessForm = new MdbCashless();
                
                cashlessForm.Show();

            }
            
            cashlessForm.Activate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            if (mQTTClientTest == null|| mQTTClientTest.IsDisposed)
            {
                mQTTClientTest = new MQTTClientTest();
                mQTTClientTest.Show();

            }
            mQTTClientTest.Activate();


        }

        private void btnTransactionStressTest_Click(object sender, EventArgs e)
        {
            if (transactionStressTest == null || transactionStressTest.IsDisposed)
            {
                transactionStressTest = new TransactionStressTest();
                
                transactionStressTest.Show();

            }
            transactionStressTest.Activate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //var factory = new ConnectionFactory() { HostName = "128.199.209.115", UserName = "admin", Password = "konbini62" };
            //using (var connection = factory.CreateConnection())
            //using (var channel = connection.CreateModel())
            //{
            //    channel.QueueDeclare(queue: "hello",
            //                         durable: false,
            //                         exclusive: false,
            //                         autoDelete: false,
            //                         arguments: null);

            //    var message =new KeyValueMessage { Key = MessageKeys.TestKey, MachineId =Guid.NewGuid(), Value = "Hello World!" };
            //    var body = MessagePackSerializer.Serialize(message);

            //    channel.BasicPublish(exchange: "",
            //                         routingKey: "hello",
            //                         basicProperties: null,
            //                         body: body);
            //    Console.WriteLine(" [x] Sent {0}", message);
            //}

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (alarmLightForm == null || alarmLightForm.IsDisposed)
            {
                alarmLightForm = new AlarmLight();
                alarmLightForm.Show();
            }
            alarmLightForm.Activate();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (paymentForm == null || paymentForm.IsDisposed)
            {
                paymentForm = new Payment();
                paymentForm.Show();

            }
            paymentForm.Activate();
        }
    }
}
