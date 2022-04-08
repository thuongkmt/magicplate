using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Products.Dtos;
using System;
using System.Threading.Tasks;

namespace KonbiCloud.Products
{
    public interface IProductAppService : IApplicationService
    {
        Task<PagedResultDto<GetProductForView>> GetAll(GetAllProductsInput input);

        Task<ProductMessage> CreateOrEdit(GetProductForEditOutput input);

        Task<GetProductForEditOutput> GetProductForEdit(EntityDto<Guid> input);

        Task Delete(EntityDto<Guid> input);

        Task<bool> UpdatePrice(ProductDto input);

        Task<bool> UpdateDisplayOrder(ProductDto input);

        Task<bool> UpdateContractorPrice(ProductDto input);

        //Task<string> UpdatePlate(ProductDto input);

        Task<bool> SyncProductData();
    }
}
