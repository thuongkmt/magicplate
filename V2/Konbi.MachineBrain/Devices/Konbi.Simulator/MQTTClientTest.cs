using System;
using System.Net;
using System.Text;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Konbi.Simulator
{
    public partial class MQTTClientTest : Form
    {
        private MqttClient mqttClient;
        string clientId = Guid.NewGuid().ToString();
        public MQTTClientTest()
        {
            InitializeComponent();
        }

        private async void btnPublish_Click(object sender, EventArgs e)
        {


            string strValue = txtPublishMessage.Text;

            // publish a message on "/home/temperature" topic with QoS 2 
            mqttClient.Publish("/home/temperature", Encoding.UTF8.GetBytes(strValue), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);

        }
        private async void MQTTClientTest_Load(object sender, EventArgs e)
        {
            // create client instance 
            //mqttClient = new MqttClient(IPAddress.Parse("3.121.19.92"));
            mqttClient = new MqttClient(IPAddress.Parse("konbiniiothub.azure-devices.net"));

            // register to message received 
            mqttClient.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            
            mqttClient.Connect(clientId);

            // subscribe to the topic "/home/temperature" with QoS 2 
            mqttClient.Subscribe(new string[] { "/home/temperature" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

        }

        static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var msg = Encoding.UTF8.GetString(e.Message);
            MessageBox.Show($"Hello world {msg}");
        }
    }
}
