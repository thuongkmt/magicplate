using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.ProductMenu.Dtos;
using System.Threading.Tasks;

namespace KonbiCloud.PlateMenus
{
    public interface IProductMenusAppService : IApplicationService 
    {
        Task<PagedResultDto<ProductMenuDto>> GetAllProductMenus(Dtos.PlateMenusInput input);

        Task<bool> UpdatePrice(Dtos.PlateMenusInput input);

        Task<bool> UpdateDisplayOrder(Dtos.PlateMenusInput input);

        Task<bool> UpdateProduct(Dtos.PlateMenusInput input);

        Task<bool> UpdatePriceStrategy(Dtos.PlateMenusInput input);
        Task<bool> UpdatePlate(AssignPlateModelInput input);
    }
}