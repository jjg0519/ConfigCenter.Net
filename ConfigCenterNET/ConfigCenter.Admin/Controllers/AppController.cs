using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ConfigCenter.Business;
using ConfigCenter.Dto;
using Webdiyer.WebControls.Mvc;
using System.Threading.Tasks;

namespace ConfigCenter.Admin.Controllers
{
    public class AppController : Controller
    {
        // GET: App
        public async Task<ActionResult> Index(int pageindex = 1, string kword = "")
        {
            var appDtos = await AppBusiness.GetAppsAsync(pageindex, 20, kword);
            return View(new PagedList<AppDto>(appDtos.Items, pageindex, 20, appDtos.TotalPage));
        }

        public async Task<JsonResult> GetAppById(int id)
        {
            return Json(await AppBusiness.GetAppByIdAsync(id), JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> DeleteAppById(int id)
        {
            ResponseResult responseResult;
            try
            {
                var result = await AppBusiness.DeleteAppByIdAsync(id);
                responseResult = new ResponseResult(result, "");
            }
            catch (Exception)
            {
                responseResult = new ResponseResult(false, "");
            }
            return Json(responseResult, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> SaveApp(AppDto appDto)
        {
            ResponseResult responseResult;
            try
            {
                await AppBusiness.SaveAppAsync(appDto);
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
