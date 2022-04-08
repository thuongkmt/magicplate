using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using RabbitMQ.Client;

namespace KonbiCloud.Messaging
{
    #region Events
    public class ConnectionStateEventArgs
    {
        public bool IsConnected { get; set; }
        public IConnectToRabbitMqService Connector { get; set; }
        public ConnectionStateEventArgs(IConnectToRabbitMqService connector, bool isConnected)
        {
            IsConnected = isConnected;
            Connector = connector;
        }

    }
    public delegate void BrokerConnectionStateChangedHandler(ConnectionStateEventArgs args);


    #endregion
    public interface IConnectToRabbitMqService : ISingletonDependency, IDisposable
    {
        bool IsConnected { get; }
        string ClientId { get; set; }
        string BrokerUrl { get; }
        Task<bool> Connect(int reconnectInterval = 10);
        /// <summary>
        ///  raised when RabbitMQ connection state changed.
        /// </summary>
        event BrokerConnectionStateChangedHandler OnBrokerConnectionStateChanged;
        IModel EnsureListeningChannel();

        IConnection GetConnection();
    }
}
