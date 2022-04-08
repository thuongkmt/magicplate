
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
using KonbiCloud.Sessions;
using Microsoft.EntityFrameworkCore;
using KonbiCloud.CloudSync;
using Abp.Domain.Uow;
using Abp.UI;
using KonbiCloud.RFIDTable.Cache;
using Abp.Runtime.Caching;
using KonbiCloud.Configuration;
using KonbiCloud.Common;
using KonbiCloud.RFIDTable;

namespace KonbiCloud.Machines
{
    [AbpAuthorize(AppPermissions.Pages_Sessions)]
    public class SessionsAppService : KonbiCloudAppServiceBase, ISessionsAppService
    {
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly ISessionsExcelExporter _sessionsExcelExporter;
        private readonly ICacheManager _cacheManager;
        private readonly IDetailLogService _detailLogService;
        private readonly ISaveFromCloudService _saveFromCloudService;
        private readonly IRfidTableSignalRMessageCommunicator _signalRCommunicator;

        public SessionsAppService(IRepository<Session, Guid> sessionRepository, ISessionsExcelExporter sessionsExcelExporter, ICacheManager cacheManager, IDetailLogService detailLog, ISaveFromCloudService saveFromCloudService, IRfidTableSignalRMessageCommunicator signalRCommunicator)
        {
            _sessionRepository = sessionRepository;
            _sessionsExcelExporter = sessionsExcelExporter;
            _cacheManager = cacheManager;
            _detailLogService = detailLog;
            _saveFromCloudService = saveFromCloudService;
            _signalRCommunicator = signalRCommunicator;
        }

        [AbpAllowAnonymous]
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

            _signalRCommunicator.NotifyProductChanges("Session");

            await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Sessions_Create)]
        private async Task Create(CreateOrEditSessionDto input)
        {
            try
            {
                var session = ObjectMapper.Map<Session>(input);
                if (AbpSession.TenantId != null)
                {
                    session.TenantId = (int?)AbpSession.TenantId;
                }
                await _sessionRepository.InsertAsync(session);
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Sessions_Edit)]
        private async Task Update(CreateOrEditSessionDto input)
        {
            try
            {
                var session = await _sessionRepository.FirstOrDefaultAsync((Guid)input.Id);
                ObjectMapper.Map(input, session);
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Sessions_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            try
            {
                await _sessionRepository.DeleteAsync(input.Id);
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
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

        [AbpAuthorize(AppPermissions.Pages_Sessions_Sync)]
        public async Task<bool> SyncSessionData()
        {           
            return await _saveFromCloudService.SyncSessionData();
        }
        public async Task<bool> SyncDataPartially()
        {
            return await _saveFromCloudService.PartiallySyncAllData();
        }
        public async Task<bool> SyncDataFully()
        {
            return await _saveFromCloudService.FullySyncAllData();
        }
    }
}