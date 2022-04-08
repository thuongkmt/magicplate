using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Services.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.Services
{
    public interface IServiceStatusAppService
    {
        Task<ListResultDto<ServiceStatusDto>> GetAllServices();
        Task<List<ServiceStatusDto>> GetAllServicesForSync();
        Task<ServiceStatusDto> UpdateService(ServiceStatusDto input);
        Task<ServiceStatusResultDto> GetServiceStatus(int id);
        Task CheckServiceStatus();
    }
}
