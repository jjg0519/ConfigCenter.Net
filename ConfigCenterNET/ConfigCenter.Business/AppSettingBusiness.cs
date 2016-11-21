using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ConfigCenter.Common;
using ConfigCenter.Dto;
using ConfigCenter.Repository;
using System.Linq.Expressions;
using System.Data.Entity;

namespace ConfigCenter.Business
{
    public class AppSettingBusiness
    {

        public static Task<List<AppSettingDto>> GetAppSettingsAsync(int appId)
        {
            return Task.Run(() =>
            {
                using (var db = new ConfigCenterConnection())
                {
                    return Mapper.Map<List<AppSetting>, List<AppSettingDto>>(db.AppSetting.Where(x => x.AppId == appId).ToList());
                }
            });
        }

        public static Task<AppSettingDtos> GetAppSettingsAsync(int appId, int pageIndex, int pageSize, string kword)
        {
            return Task.Run(() =>
            {
                using (var db = new ConfigCenterConnection())
                {
                    AppSettingDtos appSettingDtos = new AppSettingDtos();
                    Expression<Func<AppSetting, bool>> where = x => x.AppId == appId && x.ConfigKey.Contains(kword);
                    var Items = db.AppSetting.Where(where).OrderBy(o => o.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                    appSettingDtos.TotalPage = db.AppSetting.Count(where);
                    appSettingDtos.Items = Mapper.Map<List<AppSetting>, List<AppSettingDto>>(Items);
                    return appSettingDtos;
                }
            });
        }

        public static Task<AppSettingDto> GetAppSettingByIdAsync(int id)
        {
            return Task.Run(() =>
            {
                using (var db = new ConfigCenterConnection())
                {
                    return Mapper.Map<AppSetting, AppSettingDto>(db.AppSetting.SingleOrDefault(x => x.Id == id));
                }
            });
        }

        public static Task SaveAppSettingAsync(AppSettingDto appSettingDto)
        {
            return Task.Run(() =>
            {
                using (var db = new ConfigCenterConnection())
                {
                    var appSetting = Mapper.Map<AppSettingDto, AppSetting>(appSettingDto);
                    db.AppSetting.Add(appSetting);
                    db.SaveChanges();

                    var app = db.App.FirstOrDefault(x => x.Id == appSettingDto.AppId);
                    if (app != null)
                    {
                        app.Version = DateTime.Now.ToString("yyyyMMddHHmmss");
                        db.Entry(app).State = EntityState.Modified;
                        db.SaveChanges();

                        //更新zookeeper的值
                        var path = ZooKeeperHelper.ZooKeeperRootNode + "/" + app.AppId;
                        if (!ZooKeeperHelper.Exists(path))
                        {
                            ZooKeeperHelper.Create(path, null);
                        }
                        ZooKeeperHelper.SetData(path, app.Version, -1);

                    }
                }
            });
        }

        public static Task<bool> DeleteAppSettingByIdAsync(int id)
        {
            return Task.Run(() =>
            {
                using (var db = new ConfigCenterConnection())
                {
                    var appSetting = db.AppSetting.SingleOrDefault(x => x.Id == id);
                    db.AppSetting.Remove(appSetting);
                    db.SaveChanges();
                    var app = db.App.FirstOrDefault(x => x.Id == appSetting.AppId);
                    if (app != null)
                    {
                        app.Version = DateTime.Now.ToString("yyyyMMddHHmmss");
                        db.Entry(app).State = EntityState.Modified;
                        db.SaveChanges();

                        //更新zookeeper的值
                        var path = ZooKeeperHelper.ZooKeeperRootNode + "/" + app.AppId;
                        if (!ZooKeeperHelper.Exists(path))
                        {
                            ZooKeeperHelper.Create(path, null);
                        }
                        ZooKeeperHelper.SetData(path, app.Version, -1);
                    }
                    return true;
                }
            });

        }

    }
}
