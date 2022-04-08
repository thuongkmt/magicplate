using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Categories.Dtos;
using KonbiCloud.Products;

namespace KonbiCloud.Categories
{
    public interface ICategoryAppService : IApplicationService
    {
        Task<PagedResultDto<GetCategoryForView>> GetAll(GetCategoryListInput input);

        Task<GetCategoryForEditOutput> GetCategoryForEdit(EntityDto<Guid> input);

        Task CreateOrEdit(CreateOrEditCategoryDto input);

        Task Delete(EntityDto<Guid> input);
    }
}
