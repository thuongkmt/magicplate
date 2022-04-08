using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System.Threading.Tasks;
using KonbiCloud.ProductMenu.Dtos;

namespace KonbiCloud.ProductMenu
{
    public interface IProductMenusAppService : IApplicationService
    {
        Task<PagedResultDto<ProductMenuDto>> GetAllProductMenus(Dtos.PlateMenusInput input);

        Task<PagedResultDto<ProductMenuDto>> GetPOSProductMenuList(Dtos.PlateMenusInput input);

        Task<ListResultDto<PosProductMenuOutput>> GetPOSMenu(PosProductMenuInput input);

        Task<bool> UpdatePrice(Dtos.PlateMenusInput input);

        Task<bool> UpdateProduct(Dtos.PlateMenusInput input);

        Task<bool> UpdatePriceStrategy(Dtos.PlateMenusInput input);

        Task<bool> SyncProductMenuData();
        Task<bool> UpdatePlate(AssignPlateModelInput input);


    }
}