
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using KonbiCloud.Machines.Exporting;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.Dto;
using Abp.Application.Services.Dto;
using KonbiCloud.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using KonbiCloud.CloudSync;
using Abp.Domain.Uow;
using KonbiCloud.Common;
using KonbiCloud.Messaging;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Newtonsoft.Json;

namespace KonbiCloud.Machines
{
    [AbpAuthorize(AppPermissions.Pages_Sessions)]
    public class SessionsAppService : KonbiCloudAppServiceBase, ISessionsAppService
    {
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly ISessionsExcelExporter _sessionsExcelExporter;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IDetailLogService _detailLogService;
        private readonly ISendMessageToMachineService _sendMessageToMachineService;

        public SessionsAppService(IRepository<Session, Guid> sessionRepository,
            ISessionsExcelExporter sessionsExcelExporter, IRepository<Machine, Guid> machineRepository, IDetailLogService detailLog, ISendMessageToMachineService sendMessageToMachineService)
        {
            _sessionRepository = sessionRepository;
            _sessionsExcelExporter = sessionsExcelExporter;
            _machineRepository = machineRepository;
            _detailLogService = detailLog;
            _sendMessageToMachineService = sendMessageToMachineService;
        }

        public async Task<PagedResultDto<GetSessionForView>> GetAll(GetAllSessionsInput input)
        {
            try
            {
                var filteredSessions = _sessionRepository.GetAll()
                            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.FromHrs.Contains(input.Filter) || e.ToHrs.Contains(input.Filter))
                            .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.ToLower() == input.NameFilter.ToLower().Trim())
                            .WhereIf(!string.IsNullOrWhiteSpace(input.FromHrsFilter), e => e.FromHrs.ToLower() == input.FromHrsFilter.ToLower().Trim())
                            .WhereIf(!string.IsNullOrWhiteSpace(input.ToHrsFilter), e => e.ToHrs.ToLower() == input.ToHrsFilter.ToLower().Trim())
                            .WhereIf(!string.IsNullOrWhiteSpace(input.ActiveFlgFilter), e => e.ActiveFlg == bool.Parse(input.ActiveFlgFilter));


                var query = (from o in filteredSessions
                             select new GetSessionForView()
                             {
                                 Session = ObjectMapper.Map<SessionDto>(o)
                             });

                var totalCount = await query.CountAsync();

                var sessions = await query
                    .OrderBy(input.Sorting ?? "session.FromHrs asc")
                    .PageBy(input)
                    .ToListAsync();

                return new PagedResultDto<GetSessionForView>(
                    totalCount,
                    sessions
                );
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<GetSessionForView>(0, new List<GetSessionForView>());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Sessions_Edit)]
        public async Task<GetSessionForEditOutput> GetSessionForEdit(EntityDto<Guid> input)
        {
            try
            {
                var session = await _sessionRepository.FirstOrDefaultAsync(input.Id);

                var output = new GetSessionForEditOutput { Session = ObjectMapper.Map<CreateOrEditSessionDto>(session) };

                return output;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new GetSessionForEditOutput();
            }
        }

        public async Task CreateOrEdit(CreateOrEditSessionDto input)
        {
            if (input.Id == null)
            {
                await Create(input);
            }
            else
            {
                await Update(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Sessions_Create)]
        private async Task Create(CreateOrEditSessionDto input)
        {
            try
            {
                _detailLogService.Log($"Session: START create");
                var session = ObjectMapper.Map<Session>(input);

                if (AbpSession.TenantId != null)
                {
                    session.TenantId = (int?)AbpSession.TenantId;
                }
                await _sessionRepository.InsertAsync(session);
                await CurrentUnitOfWork.SaveChangesAsync();
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.Session });
                _detailLogService.Log($"Session: END create, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Session: END create with error -> " + ex.ToString());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Sessions_Edit)]
        private async Task Update(CreateOrEditSessionDto input)
        {
            try
            {
                _detailLogService.Log($"Session: START update");
                var session = await _sessionRepository.FirstOrDefaultAsync((Guid)input.Id);
                ObjectMapper.Map(input, session);
                await CurrentUnitOfWork.SaveChangesAsync();
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.Session });
                _detailLogService.Log($"Session: END update, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Session: END update with error -> " + ex.ToString());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Sessions_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            try
            {
                _detailLogService.Log($"Session: START delete");
                await _sessionRepository.DeleteAsync(input.Id);
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() {Key = MessageKeys.Session});
                _detailLogService.Log($"Session: END delete, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Session: END delete with error -> " + ex.ToString());
            }
        }

        public async Task<FileDto> GetSessionsToExcel(GetAllSessionsForExcelInput input)
        {

            var filteredSessions = _sessionRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.FromHrs.Contains(input.Filter) || e.ToHrs.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.ToLower() == input.NameFilter.ToLower().Trim())
                        .WhereIf(!string.IsNullOrWhiteSpace(input.FromHrsFilter), e => e.FromHrs.ToLower() == input.FromHrsFilter.ToLower().Trim())
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ToHrsFilter), e => e.ToHrs.ToLower() == input.ToHrsFilter.ToLower().Trim());


            var query = (from o in filteredSessions
                         select new GetSessionForView()
                         {
                             Session = ObjectMapper.Map<SessionDto>(o)
                         });


            var sessionListDtos = await query.ToListAsync();

            return _sessionsExcelExporter.ExportToFile(sessionListDtos);
        }

        [AbpAllowAnonymous]
        public async Task<EntitySyncOutputDto<SessionSyncDto>> GetSessions(EntitySyncInputDto<Guid> machineSyncInput)
        {
            try
            {
                _detailLogService.Log($"Session: START get session from server to sync to slave, request -> {JsonConvert.SerializeObject(machineSyncInput)}");
                var unsyncedEntities = new List<Session>();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineSyncInput.Id);
                    if(machine == null)
                    {
                        _detailLogService.Log($"Session: MachineId is {machineSyncInput.Id} does not exist");
                        return null;
                    }
                    else if (machine.IsDeleted)
                    {
                        _detailLogService.Log($"Session: MachineId is {machineSyncInput.Id} was deleted");
                        return null;
                    }
                    unsyncedEntities = await _sessionRepository.GetAllListAsync(x => x.TenantId == machine.TenantId &&
                                                                       // get data with last synced timestamp
                                                                       (x.CreationTime > machineSyncInput.LastSynced ||
                                                                           x.LastModificationTime > machineSyncInput.LastSynced ||
                                                                           x.DeletionTime > machineSyncInput.LastSynced
                                                                       ));
                }
                var output = new EntitySyncOutputDto<SessionSyncDto>();
                foreach (var entity in unsyncedEntities)
                {
                    if (entity.IsDeleted)
                    {
                        output.DeletionEntities.Add(new SessionSyncDto()
                        {
                            Id = entity.Id
                        });
                    }
                    else
                    {
                        output.ModificationEntities.Add(new SessionSyncDto()
                        {
                            Id = entity.Id,
                            Name = entity.Name,
                            ActiveFlg = entity.ActiveFlg,
                            FromHrs = entity.FromHrs,
                            ToHrs = entity.ToHrs
                        });
                    }
                    // calculate latest sync time, so that client can keep track
                    //var recordLastUpdated = new[] { cat.CreationTime, cat.LastModificationTime, cat.DeletionTime }.Max();

                    // ignore this because LastSyncedTimeStamp is set to Datetime.Now currently.
                    //if (output.LastSyncedTimeStamp < recordLastUpdated?.ToUnixTime())
                    //{
                    //    output.LastSyncedTimeStamp = recordLastUpdated.GetValueOrDefault().ToUnixTime();
                    //}
                }
                _detailLogService.Log($"Session: END get session from server to sync to slave, data -> " + JsonConvert.SerializeObject(output));
                return output;
            }
            catch (Exception ex)
            {

                _detailLogService.Log($"Session: END get session from server to sync to slave, error -> " + ex.ToString());
                return null;
            }
        }

    }
}