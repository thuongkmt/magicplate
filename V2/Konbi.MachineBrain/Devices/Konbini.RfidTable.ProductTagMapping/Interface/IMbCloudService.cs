using Konbini.RfidFridge.TagManagement.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Konbini.RfidFridge.TagManagement.DTO.TrayResult;

namespace Konbini.RfidFridge.TagManagement.Interface
{
    public interface IMbCloudService
    {
        string BASE_URL { get; set; }
        string USER_NAME { get; set; }
        string PASSWORD { get; set; }
        string Token { get; set; }
        Task<List<PlateCategoryDTO.PlateCategory>> GetPlateCategories();
        Task<List<GetPlateForView>> GetAllTray();
        Task<string> GetToken();
    }
}
