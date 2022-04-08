using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using KonbiBrain.Common.Messages;
using KonbiBrain.Messages;
using KonbiCloud.Common;
using Konbini.Messages.Commands;
using Konbini.Messages.Commands.RFIDTable;
using Konbini.Messages.Enums;
using Konbini.Messages.Payment;
using Newtonsoft.Json;
using NsqSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    public class AlarmLightManager : IAlarmLightManager, IHandler
    {
        private readonly IMessageProducerService nsqProducerService;
        private readonly IDetailLogService detailLogger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Services.Service> _serviceRepository;
        private readonly Consumer consumer;

        private CancellationTokenSource ctx;
        private object commandLock = new object();
        private IUniversalCommands Command { get; set; }

        public AlarmLightManager(
           IMessageProducerService nsqProducerService,
           IDetailLogService detailLogger,
           IUnitOfWorkManager unitOfWorkManager,
           IRepository<Services.Service> serviceRepository
        )
        {
            this.nsqProducerService = nsqProducerService;
            this.detailLogger = detailLogger;
            _unitOfWorkManager = unitOfWorkManager;
            _serviceRepository = serviceRepository;

            consumer = new Consumer(NsqTopics.ALARM_LIGHT_RESPONSE_TOPIC, NsqConstants.NsqDefaultChannel, new Config() { ClientID = Guid.NewGuid().ToString() });

            consumer.AddHandler(this);
            // use ConnectToNsqd instead of using ConnectToNsqLookupd because  we use standalone nsq service not cluster one.
            consumer.ConnectToNsqd(NsqConstants.NsqUrlConsumer);
        }
        
        public bool Switch(bool green, bool red, bool beep, bool blink, int duration, string soundIntruction)
        {
            detailLogger.Log($"AlarmLight: Green: {green}, Red: {red}, Beep: {beep}, Blink: {blink}, Duration: {duration}, Sound: {soundIntruction}");
            var cmd = new AlarmLightCommand()
            {
                Green = green,
                Red = red,
                Beep = beep,
                Blink = blink,
                Duration = duration,
                SoundIntruction = soundIntruction
            };
            var result =  nsqProducerService.SendNsqCommand(NsqTopics.ALARM_LIGHT_REQUEST_TOPIC, cmd);
            detailLogger.Log($"AlarmLight: Send NSQ result: {result}");
            return result;
        }
        public bool Off()
        {
            detailLogger.Log($"AlarmLight: Off - switch off all lights and sound");
            var cmd = new AlarmLightCommand()
            {
                Off = true
            };
            var result = nsqProducerService.SendNsqCommand(NsqTopics.ALARM_LIGHT_REQUEST_TOPIC, cmd);
            detailLogger.Log($"AlarmLight: Send NSQ result: {result}");
            return result;
        }

        public async Task<bool> PingAsync(string args = "")
        {
            var cmd = new PingCommand();
            cmd.CommandId = Guid.NewGuid();
            Command = cmd;
            var result = await SendCommandAsync();
            return result == CommandState.Received;
        }
        private async Task<CommandState> SendCommandAsync()
        {
            ctx?.Cancel();
            lock (commandLock)
            {

                var result = nsqProducerService.SendNsqCommand(NsqTopics.ALARM_LIGHT_REQUEST_TOPIC, Command);
                if (result)
                {
                    Command.CommandState = CommandState.SendSuccess;
                }
                else
                {
                    Command.CommandState = CommandState.Failed;

                }

            }
            ctx = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {

                while (Command.CommandState == CommandState.SendSuccess && !ctx.Token.IsCancellationRequested)
                {
                    await Task.Delay(50, ctx.Token);
                }
                if (ctx.Token.IsCancellationRequested)
                    Command.CommandState = CommandState.Cancelled;

            }
            catch (OperationCanceledException)
            {
                Command.CommandState = CommandState.TimeOut;

            }

            return Command.CommandState;
        }
        private void UpdateServiceStatus(DeviceInfoCommandPayload payload)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin())
            {
                try
                {
                    var service = _serviceRepository.FirstOrDefault(el => el.Type == payload.Type.Trim());
                    if (service != null)
                    {
                        service.Name = payload.Name.Trim();
                        service.IsError = payload.HasError;
                        service.ErrorMessage = payload.Errors.FirstOrDefault();
                    }
                    else
                    {
                        _serviceRepository.Insert(new Services.Service() { Name = payload.Name.Trim(), Type = payload.Type.Trim(), IsError = payload.HasError, ErrorMessage = payload.Errors.FirstOrDefault() });
                    }
                }
                catch (Exception ex)
                {
                    detailLogger.Log("ERROR: AlarmLightManager.UpdateServiceStatus");

                    detailLogger.Log(ex.Message);
                }

                finally
                {
                    unitOfWork.Complete();
                }

            }
        }

        public void HandleMessage(IMessage message)
        {
            // process for ACK message where  telling that the request  has been sent to device successfully.
            try
            {
                var msg = Encoding.UTF8.GetString(message.Body);
                var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);
                // ignore expired messages
                if (obj.IsTimeout())
                    return;               
                if (obj.Command == UniversalCommandConstants.ACKResponse)
                {
                    if (Command != null && Command.CommandId == obj.CommandId)
                    {
                        Command.CommandState = CommandState.Received;
                    }
                }
              
                else if (obj.Command == UniversalCommandConstants.DeviceInfo)
                {
                    var Command = JsonConvert.DeserializeObject<DeviceInfoCommand>(msg);
                    UpdateServiceStatus(Command.CommandObject);


                }              
            }
            catch
            {
                // dont catch unexpected message

            }
        }

        public void LogFailedMessage(IMessage message)
        {
            //throw new NotImplementedException();
        }
    }
}
