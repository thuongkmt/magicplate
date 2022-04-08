using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.UI;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Messaging;
using KonbiCloud.RFIDTable;
using KonbiCloud.Services.Dto;
using KonbiCloud.StatusConnectServer;
using Konbini.Messages.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KonbiCloud.Services
{
    public class ServiceStatusAppService : KonbiCloudAppServiceBase, IServiceStatusAppService
    {
        private readonly IRepository<Service,int> _serviceRepository;
        private readonly IConnectToRabbitMqService _connector;
        private readonly IStatusConnectServerService _statusConnectServerService;
        private readonly ITableSettingsManager _tableSettingsManager;
        private readonly ITableManager _tableManager;
        private readonly IPaymentManager _paymentManager;
        private readonly ISettingManager _settingManager;
        private readonly IAlarmLightManager _alarmLightManager;
        private readonly IRfidTableSignalRMessageCommunicator _signalRCommunicator;

        public ServiceStatusAppService(IRepository<Service,int> serviceRepository,
            IConnectToRabbitMqService connectToRabbitMqService,
            IStatusConnectServerService statusConnectServerService,
            IPaymentManager paymentManager,
            ITableManager tableManager,
            ITableSettingsManager tableSettingsManager,
            IAlarmLightManager alarmLightManager,
             ISettingManager settingManager,
             IRfidTableSignalRMessageCommunicator signalRCommunicator)
        {
            _serviceRepository = serviceRepository;
            _connector = connectToRabbitMqService;
            _statusConnectServerService = statusConnectServerService;
            _tableSettingsManager = tableSettingsManager;
            _paymentManager = paymentManager;
            _settingManager = settingManager;
            _alarmLightManager = alarmLightManager;
            _tableManager = tableManager;
            _signalRCommunicator = signalRCommunicator;
        }

        public async Task<ListResultDto<ServiceStatusDto>> GetAllServices()
        {
            var result = new List<ServiceStatusDto>();
            var services = await _serviceRepository.GetAll().ToListAsync();

            foreach (var item in services)
            {
                result.Add(new ServiceStatusDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Type = item.Type,
                    IsArchived = item.IsArchived,
                });
            }

            return new ListResultDto<ServiceStatusDto>(result);
        }

        public async Task<List<ServiceStatusDto>> GetAllServicesForSync()
        {
            var result = new List<ServiceStatusDto>();
            var services = await _serviceRepository.GetAll().ToListAsync();

            foreach (var item in services)
            {
                result.Add(new ServiceStatusDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Type = item.Type,
                    IsArchived = item.IsArchived,
                });
            }

            return new List<ServiceStatusDto>(result);
        }

        public async Task<ServiceStatusDto> UpdateService(ServiceStatusDto input)
        {
            var service = _serviceRepository.FirstOrDefault(x => x.Id == input.Id);

            if(service == null)
            {
                throw new UserFriendlyException("Can not find service with id = " + input.Id);
            }

            service.IsArchived = input.IsArchived;
            await _serviceRepository.UpdateAsync(service);

            return new ServiceStatusDto {
                Id = service.Id,
                Name = service.Name,
                Type = service.Type,
                IsArchived = service.IsArchived
            };
        }

        public async Task<ServiceStatusResultDto> GetServiceStatus(int id)
        {
            var result = new ServiceStatusResultDto();
            var service = _serviceRepository.FirstOrDefault(x => x.Id == id);

            if (service == null)
            {
                throw new UserFriendlyException("Could not find service with id = " + id);
            }

            switch (service.Type)
            {
                case ServiceTypeConstants.RABBITMQ_SERVER:
                    result.Status = _connector.IsConnected;
                    if (!result.Status)
                        result.Message = $"Could not connect to rabbitmq server <{_connector.BrokerUrl}>";
                    else
                    {
                        result.Message = $"Connected to rabbitmq server <{_connector.BrokerUrl}>";
                    }
                    break;
                case ServiceTypeConstants.MAGIC_PLATE_SERVER:
                    result.Status = await _statusConnectServerService.GetStatusConnectServer();
                    var serverUrl = _settingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                    if (!result.Status)
                    {
                       
                        result.Message = $"Could not connect to MagicPlate Server <{serverUrl}>";
                    }
                    else
                    {
                        result.Message = $"Connected to server at <{serverUrl}>";
                    }
                    break;
                case ServiceTypeConstants.TAG_READER:
                    result.Status= await _tableSettingsManager.PingAsync();
                    if (!result.Status)
                    {
                        result.Message = $"{service.Name} service is not running";
                    }
                    else
                    {
                        service = _serviceRepository.GetAll().Where(el => el.Type == service.Type).FirstOrDefault();
                        if (service != null)
                        {
                            if (service.IsError)
                                result.Status = false;
                            result.Message = service.ErrorMessage;
                        }
                       
                    }
                    break;
                case ServiceTypeConstants.LIGHT_ALARM:
                    result.Status = await _alarmLightManager.PingAsync();
                    if (!result.Status)
                    {
                        result.Message = $"{service.Name} service is not running";
                    }
                    else
                    {
                        service = _serviceRepository.GetAll().Where(el => el.Type == service.Type).FirstOrDefault();
                        if (service != null)
                        {
                            if (service.IsError)
                                result.Status = false;
                            result.Message = service.ErrorMessage;
                        }

                    }
                    break;
                case ServiceTypeConstants.CAMERA:
                    result.Status = await _tableManager.PingAsync();
                    if (!result.Status)
                    {
                        result.Message = $"{service.Name} service is not running";
                    }
                    else
                    {
                        service = _serviceRepository.GetAll().Where(el => el.Type == service.Type).FirstOrDefault();
                        if (service != null)
                        {
                            if (service.IsError)
                                result.Status = false;
                            result.Message = service.ErrorMessage;
                        }

                    }
                    break;
                case ServiceTypeConstants.PAYMENT_QR_CONTROLLER:
                case ServiceTypeConstants.KONBI_CREDITS_PAYMENT_CONTROLLER:
                case ServiceTypeConstants.PAYMENT_CONTROLLER:
                    result.Status = await _paymentManager.PingAsync(service.Type);
                    if (!result.Status)
                    {
                        result.Message = $"{service.Name} service is not running";
                    }
                    else
                    {
                        service = _serviceRepository.GetAll().Where(el => el.Type == service.Type).FirstOrDefault();
                        if (service != null)
                        {
                            if (service.IsError)
                                result.Status = false;
                            result.Message = service.ErrorMessage;
                        }

                    }
                    break;
                default:
                    result.Status = false;
                    break;
            }

            return result;
        }

        ////////////////////////////////////////
        /// Description: check servie status of Server connection, RFID signal, LighrAlarm, IUC reader, ....
        /// Author: nnthuong
        /// Date: 2021/01/25
        /// /////////////////////////////////////
        public async Task CheckServiceStatus()
        {
            
            var services = await _serviceRepository.GetAll().ToListAsync();
            var serviceStatusForCheckingOnUIList = new List<ServiceStatusForCheckingOnUIDto>();
            foreach (var service in services)
            {
                var serviceStatusResultDtos = await GetServiceStatus(service.Id);

                serviceStatusForCheckingOnUIList.Add(new ServiceStatusForCheckingOnUIDto
                {
                    Status = serviceStatusResultDtos.Status,
                    Message = serviceStatusResultDtos.Message,
                    Type = service.Type
                });
            }
            await _signalRCommunicator.CheckServiceStatus(serviceStatusForCheckingOnUIList);
        }

    }
}
