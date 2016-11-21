using ConfigCenter.Business;
using ConfigCenter.Dto;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace ConfigCenter.Admin.Controllers
{
    public class AppSettingController : Controller
    {
        // GET: App
        public async Task<ActionResult> Index(int pageindex = 1, int appId = 0, string kword = "")
        {
            var appSettingDtos = await AppSettingBusiness.GetAppSettingsAsync(appId, pageindex, 20, kword);
            return View(new PagedList<AppSettingDto>(appSettingDtos.Items, pageindex, 20, appSettingDtos.TotalPage));
        }

        public async Task<JsonResult> GetAppSettingById(int id)
        {
            return Json(await AppSettingBusiness.GetAppSettingByIdAsync(id), JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> DeleteAppSettingById(int id)
        {
            ResponseResult responseResult;
            try
            {
                var result = await AppSettingBusiness.DeleteAppSettingByIdAsync(id);
                responseResult = new ResponseResult(result, "");
            }
            catch (Exception)
            {
                responseResult = new ResponseResult(false, "");
            }
            return Json(responseResult, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> SaveAppSetting(AppSettingDto appSettingDto)
        {
            ResponseResult responseResult;
            try
            {
                await AppSettingBusiness.SaveAppSettingAsync(appSettingDto);
                responseResult = new ResponseResult(true, "");
            }
            catch (Exception)
            {
                responseResult = new ResponseResult(false, "");
            }
            return Json(responseResult, JsonRequestBehavior.AllowGet);
        }
    }
}