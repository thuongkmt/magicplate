using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Abp.Configuration;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using Konbini.Messages;
using MessagePack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace KonbiCloud.Messaging
{
   
    
    public class ConnectToRabbitMqService : IConnectToRabbitMqService
    {
        private bool _isConnected = false;
        private string _clientId = "MagicPlateCloud";
        private IConnection _connection;
        private IModel _publishingChannel;
        private ILogger _logger;
        private ConnectionFactory _connectionFactory = new ConnectionFactory();
        private bool _isConnecting;
        private readonly IConfigurationRoot _configurationRoot;
        public string ClientId
        {
            get
            {
                return _clientId;
            }
        }

        public event BrokerConnectionStateChangedHandler OnBrokerConnectionStateChanged;

        public string BrokerUrl
        {
            get
            {
                if (_connectionFactory != null)
                    return _connectionFactory.HostName;
                return string.Empty;
            }
        }

        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }

        

        public ConnectToRabbitMqService(IAppConfigurationAccessor configurationRoot,
            ILogger logger, ISettingManager settingManager)
        {
          
            _configurationRoot = configurationRoot.Configuration;            
            _logger = logger;
        }



        public async Task<bool> Connect(int reconnectInterval = 10)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    if (_isConnecting)
                    {
                        return false;
                    }
                    _isConnecting = true;
                    var hostName = _configurationRoot["RabbitMQ:HostName"];
                    var username = _configurationRoot["RabbitMQ:UserName"];
                    var pwd = _configurationRoot["RabbitMQ:Password"];

                    _connectionFactory.HostName = hostName;
                    _connectionFactory.UserName = username;
                    _connectionFactory.Password = pwd;

                    _connectionFactory.AutomaticRecoveryEnabled = true;
                    _connectionFactory.TopologyRecoveryEnabled = true;

                    _connectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(reconnectInterval);
                    _logger.Info($"Connecting to broker... clientId: {ClientId}, host: {_connectionFactory.HostName}, username:{_connectionFactory.UserName}, password:{_connectionFactory.Password}");
                    _connection = _connectionFactory.CreateConnection();

                    //define callbacks on significant signal that connection can send. just  for debugging how connection recovery acts.
                    _connection.CallbackException += Connection_CallbackException;
                    _connection.ConnectionShutdown += Connection_ConnectionShutdown;
                    _connection.ConnectionRecoveryError += Connection_ConnectionRecoveryError;
                    _connection.RecoverySucceeded += _connection_RecoverySucceeded;

                    


                    _logger.Info($"RabbitMQ: clientId: {ClientId}, connected sucessfully");
                    var channel = EnsurePublishingChannel();
                    
                    _isConnected = true;
                    OnBrokerConnectionStateChanged?.Invoke(new ConnectionStateEventArgs(this, _isConnected));
                    _isConnecting = false;
                    return true;


                }
                catch (BrokerUnreachableException e)
                {                    
                    _logger.Error(e.Message);
                    _logger.Info($"RabbitMQ: clientId: {ClientId}, Reconnect after {reconnectInterval} seconds");
                    await Task.Delay(reconnectInterval * 1000);
                    await Connect(reconnectInterval);

                }
                catch(Exception ex)
                {
                  
                    _logger.Error(ex.Message);
                    _logger.Info($"RabbitMQ: clientId: {ClientId}, Reconnect after {reconnectInterval} seconds");
                    await Task.Delay(reconnectInterval * 1000);
                    await Connect(reconnectInterval);
                }
                finally{
                    _isConnecting = false;
                }
                return false;
            });
        }

        private void _connection_RecoverySucceeded(object sender, EventArgs e)
        {
            _isConnected = true;
            OnBrokerConnectionStateChanged?.Invoke(new ConnectionStateEventArgs(this, _isConnected));
        }

        private void Connection_ConnectionRecoveryError(object sender, ConnectionRecoveryErrorEventArgs e)
        {
            _logger.Error($"RabbitMQ: clientId: {ClientId}, Connection_ConnectionRecoveryError: {e.Exception.Message}");
            if (_isConnected)
            {
                _isConnected = false;
                OnBrokerConnectionStateChanged?.Invoke(new ConnectionStateEventArgs(this, _isConnected));
            }
        }

        private void Connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.Error($"RabbitMQ: clientId: {ClientId}, Connection_ConnectionShutdown: {e.ReplyCode}");
            if (_isConnected)
            {
                _isConnected = false;
                OnBrokerConnectionStateChanged?.Invoke(new ConnectionStateEventArgs(this, _isConnected));
            }
        }

        private void Connection_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_isConnected)
            {
                _isConnected = false;
                OnBrokerConnectionStateChanged?.Invoke(new ConnectionStateEventArgs(this, _isConnected));
            }
            
            _logger.Error($"RabbitMQ: clientId: {ClientId}, Connection_CallbackException: {e.Exception.Message}");
        }

        public IModel EnsurePublishingChannel()
        {
            if (_connection == null || !_connection.IsOpen)
                return null;
            if (_publishingChannel != null && !_publishingChannel.IsOpen)
            {
                _publishingChannel.Close();
                _publishingChannel = _connection.CreateModel();
            }
            if (_publishingChannel == null)
                _publishingChannel = _connection.CreateModel();
            return _publishingChannel;
        }

       

        public IConnection GetConnection()
        {
            return _connection;
        }


        public void Dispose()
        {
            _logger?.Info("RabbitMQ disposed");
            _connection?.Dispose();
            _publishingChannel?.Dispose();            
            
        }


    }
}
