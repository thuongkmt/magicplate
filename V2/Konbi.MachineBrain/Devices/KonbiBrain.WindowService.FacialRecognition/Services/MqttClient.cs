using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowService.FacialRecognition.Services
{
    public class MqttClient
    {
        public IManagedMqttClient client;
        private string _mqttURI = "";
        private string _mqttUser = "";
        private string _mqttPassword = "";
        private int _mqttPort = 0;

        public MqttClient(string mqttURI, string mqttUser, string mqttPassword, int mqttPort)
        {
            _mqttURI = mqttURI;
            _mqttUser = mqttUser;
            _mqttPassword = mqttPassword;
            _mqttPort = mqttPort;
            CreateClient();
        }

        public IManagedMqttClient CreateClient()
        {
            return client = new MqttFactory().CreateManagedMqttClient();
        }

        /// <summary>
        /// Connection to MQTT.
        /// </summary>
        /// <returns>Task.</returns>
        /// 
        public async Task ConnectAsync()
        {
            string clientId = Guid.NewGuid().ToString();
            string mqttURI = _mqttURI;
            string mqttUser = _mqttUser;
            string mqttPassword = _mqttPassword;
            int mqttPort = _mqttPort;
            bool mqttSecure = false;

            var messageBuilder = new MqttClientOptionsBuilder()
            .WithClientId(clientId)
            .WithCredentials(mqttUser, mqttPassword)
            .WithTcpServer(mqttURI, mqttPort)
            .WithCleanSession();

            var options = mqttSecure
                ? messageBuilder
                .WithTls()
                .Build()
                : messageBuilder
                .Build();

            var managedOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(options)
                .Build();

            await client.StartAsync(managedOptions);
        }
        /// <summary>
        /// Subscribe topic.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="qos">Quality of Service. {1 - received all message even in the past, 0 - will received only the message while the app is online}</param>
        /// <returns>Task.</returns>
        [Obsolete]
        public async Task SubscribeAsync(string topic, int qos = 0) =>
          await client.SubscribeAsync(new TopicFilterBuilder()
            .WithTopic(topic)
            .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
            .Build());
    }
    public class MqttTopic
    {
        public static string MESSAGE_CREATE_FACEPRINT_TOPIC = "MESSAGE_CREATE_FACEPRINT_TOPIC";
        public static string MESSAGE_DELETE_FACEPRINT_TOPIC = "MESSAGE_DELETE_FACEPRINT_TOPIC";
        public static string MESSAGE_OVERRIDE_FACEPRINT_TOPIC = "MESSAGE_OVERRIDE_FACEPRINT_TOPIC";
    }
}
