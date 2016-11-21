using ConfigCenter.Business;
using ConfigCenter.Dto;
using ServiceStack;
using System.Threading.Tasks;

namespace ConfigCenter.Api.Services
{
    public class ConfigCenterService : Service
    {
        public async Task<object> Get(GetAppVersion getAppVersion)
        {
            return new GetAppVersionResponse() { AppDtos = await AppBusiness.GetAppVersionsAsync(getAppVersion.AppIds) };
        }


        public async Task<object> Get(GetAppSettings getAppSettings)
        {
            return new GetAppSettingsResponse() { AppSettings = await AppSettingBusiness.GetAppSettingsAsync(getAppSettings.AppId) };
        }

    }
}