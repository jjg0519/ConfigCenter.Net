using AutoMapper;
using ConfigCenter.Dto;
using ConfigCenter.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ConfigCenter.Business
{
    public class AppBusiness
    {

        public static Task<AppDtos> GetAppsAsync(int pageIndex, int pageSize, string kword)
        {
            return Task.Run(() =>
            {
                using (var db = new ConfigCenterConnection())
                {
                    AppDtos appDtos = new AppDtos();
                    Expression<Func<App, bool>> where = x => x.AppId.Contains(kword);
                    var Items = db.App.Where(where).OrderBy(o => o.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                    appDtos.Items = Mapper.Map<List<App>, List<AppDto>>(Items); ;
                    appDtos.TotalPage = db.App.Count(where);
                    return appDtos;
                }
            });
        }

        public static Task<AppDto> GetAppByIdAsync(int Id)
        {
            return Task.Run(() =>
            {
                using (var db = new ConfigCenterConnection())
                {
                    return Mapper.Map<App, AppDto>(db.App.SingleOrDefault(x => x.Id == Id));
                }
            });
        }

        public static Task<AppDto> GetAppVersionAsync(string appId)
        {
            return Task.Run(() =>
            {
                using (var db = new ConfigCenterConnection())
                {
                    return Mapper.Map<App, AppDto>(db.App.SingleOrDefault(x => x.AppId == appId));
                }
            });
        }

        public static Task<List<AppDto>> GetAppVersionsAsync(string[] appIds)
        {
            return Task.Run(() =>
            {
                using (var db = new ConfigCenterConnection())
                {
                    return Mapper.Map<List<App>, List<AppDto>>(db.App.Where(x => appIds.Contains(x.AppId)).ToList());
                }
            });
        }


        public static Task SaveAppAsync(AppDto appDto)
        {
            return Task.Run(() =>
            {
                using (var db = new ConfigCenterConnection())
                {
                    var app = Mapper.Map<AppDto, App>(appDto);
                    db.App.Add(app);
                    db.SaveChanges();
                }
            });
        }

        public static Task<bool>  DeleteAppByIdAsync(int id)
        {
            return Task.Run(() =>
            {
                using (var db = new ConfigCenterConnection())
                {
                    db.App.Remove(db.App.SingleOrDefault(x => x.Id == id));
                    db.SaveChanges();
                    return true;
                }
            });
        }

    }
}