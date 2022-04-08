using Konbi.RealsenseID.Util;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbi.RealsenseID.Services
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
        }
        /// <summary>
        /// Connection to MQTT.
        /// </summary>
        /// <returns>Task.</returns>
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

            client = new MqttFactory().CreateManagedMqttClient();
            await client.StartAsync(managedOptions);
        }

        /// <summary>
        /// Publish Message.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="payload">Payload.</param>
        /// <param name="retainFlag">Retain flag.</param>
        /// <param name="qos">Quality of Service.</param>
        /// <returns>Task.</returns>
        public async Task PublishAsync(string topic, string payload, bool retainFlag = true, int qos = 1) =>
          await client.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
            .WithRetainFlag(retainFlag)
            .Build());
    }
    public class MqttTopic
    {
        public static string MESSAGE_CREATE_FACEPRINT_TOPIC = "MESSAGE_CREATE_FACEPRINT_TOPIC";
        public static string MESSAGE_DELETE_FACEPRINT_TOPIC = "MESSAGE_DELETE_FACEPRINT_TOPIC";
        public static string MESSAGE_OVERRIDE_FACEPRINT_TOPIC = "MESSAGE_OVERRIDE_FACEPRINT_TOPIC";
    }
}
