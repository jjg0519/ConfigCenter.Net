using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Configuration;
using ConfigCenter.Dto;
using ServiceStack;
using ZooKeeperNet;
using ZKClientNET.Client;
using ZKClientNET.Serialize;
using ZKClientNET.Listener;
using System.Collections.Concurrent;

namespace ConfigCenter
{
    public class ConfigCenter
    {

        private static ConcurrentDictionary<string, string> currentVersionDic = new ConcurrentDictionary<string, string>();

        //private static Task _task;

        private static readonly JsonServiceClient Client = new JsonServiceClient(ConfigurationManager.AppSettings["ServiceClient"]);

        private static ZKClient zkClient;

        static ConfigCenter()
        {
            var appVersionResponse = Client.Get(new GetAppVersion { AppIds = new string[] { "ZookeeperConfig" } });

            if (appVersionResponse.AppDtos != null && appVersionResponse.AppDtos.Count > 0)
            {
                var appDto = appVersionResponse.AppDtos[0];
                var appSettings = Client.Get(new GetAppSettings { AppId = appDto.Id });
                if (appSettings != null)
                {
                    foreach (AppSettingDto appSetting in appSettings.AppSettings)
                    {
                        if (appSetting.ConfigKey == "ZookeeperAddress")
                        {
                            zkClient = ZKClientBuilder.NewZKClient(appSetting.ConfigValue)
                             .SessionTimeout(10000)
                             .ConnectionTimeout(10000)
                             .Serializer(new SerializableSerializer())
                             .Build();
                        }
                    }
                }
            }
            if (zkClient == null)
            {
                zkClient = ZKClientBuilder.NewZKClient(ConfigurationManager.AppSettings["ZookeeperAddress"])
                            .SessionTimeout(10000)
                            .ConnectionTimeout(10000)
                            .Serializer(new SerializableSerializer())
                            .Build();
            }
        }

        public static void Init(params string[] appIds)
        {
            SyncVersion(appIds);  //先同步获取一次，保证global之后执行的代码的都能获取到配置

            if (zkClient != null)   //如果zookeeper可用，则注册zookeeper
            {
                ZKDataListener zkDataListener = new ZKDataListener()
               .DataChange((dataPath, data) =>
                {
                    SyncVersion((dataPath.Split('/'))[2]);
                });
                foreach (var appId in appIds)
                {
                    zkClient.SubscribeDataChanges("/ConfigCenter/" + appId, zkDataListener);
                }
            }

            //_task = new Task(SyncVersion, appId, 20000, 10000);  //如果zookeeper不可用，同时为了保证在zookeeper挂掉后，有补偿措施，启动一个定时获取的任务。

        }

        public static void Stop()
        {
            if (zkClient != null)
            {
                zkClient.Close();
            }            
            //_task.Stop();
        }

        private static void SyncVersion(params string[] appIds)
        {
            foreach (var appId in appIds)
            {
                if (!currentVersionDic.ContainsKey(appId))
                {
                    currentVersionDic.TryAdd(appId, string.Empty);
                }
            }
            var appVersionResponse = Client.Get(new GetAppVersion { AppIds = appIds });
            if (appVersionResponse.AppDtos != null && appVersionResponse.AppDtos.Count > 0)
            {
                foreach (var appDto in appVersionResponse.AppDtos)
                {
                    if (currentVersionDic[appDto.AppId] != appDto.Version) //客户端保存的版本号和服务端的版本号不一致，需要客户端去更新
                    {
                        var appSettings = Client.Get(new GetAppSettings { AppId = appDto.Id });
                        if (appSettings != null)
                        {
                            var syncSuccess = SyncLocalSetting(appSettings.AppSettings);
                            if (syncSuccess)
                            {
                                currentVersionDic[appDto.AppId] = appDto.Version;
                            }
                        }
                    }
                }
            }
        }

        private static bool SyncLocalSetting(List<AppSettingDto> appSettingDtos)
        {
            try
            {
                var config = WebConfigurationManager.OpenWebConfiguration("/");

                foreach (var appSettingDto in appSettingDtos)
                {
                    if (appSettingDto.ConfigType == 0) //普通配置节点
                    {
                        if (!ConfigurationManager.AppSettings.AllKeys.Contains(appSettingDto.ConfigKey))
                        {
                            config.AppSettings.Settings.Add(appSettingDto.ConfigKey, appSettingDto.ConfigValue);
                        }
                        else
                        {
                            if (ConfigurationManager.AppSettings[appSettingDto.ConfigKey] != appSettingDto.ConfigValue)
                            {
                                config.AppSettings.Settings[appSettingDto.ConfigKey].Value = appSettingDto.ConfigValue;
                            }
                        }
                    }
                    else //connection配置节点
                    {
                        if (ConfigurationManager.ConnectionStrings[appSettingDto.ConfigKey] == null)
                        {
                            config.ConnectionStrings.ConnectionStrings.Add(
                                new ConnectionStringSettings(appSettingDto.ConfigKey, appSettingDto.ConfigValue));
                        }
                        else
                        {
                            if (ConfigurationManager.ConnectionStrings[appSettingDto.ConfigKey].ConnectionString !=
                                appSettingDto.ConfigValue)
                                config.ConnectionStrings.ConnectionStrings[appSettingDto.ConfigKey].ConnectionString =
                                    appSettingDto.ConfigValue;
                        }
                    }
                }

                //var localKeys = ConfigurationManager.AppSettings.AllKeys.ToList();
                //var remoteKeys = (from appSettingDto in appSettingDtos
                //                  where appSettingDto.ConfigType == 0
                //                  select appSettingDto.ConfigKey).ToList();
                ////删除setting节点
                //var settingdiffs = GetDiff(localKeys, remoteKeys).Where(x => x != "ServiceClient" && x != "ZookeeperAddress").ToList();
                //foreach (var settingdiff in settingdiffs)
                //{
                //    config.AppSettings.Settings.Remove(settingdiff);
                //}

                //var localKeys1 = (from ConnectionStringSettings connectionStringSetting in ConfigurationManager.ConnectionStrings
                //                  select connectionStringSetting.Name).ToList();
                //var remoteKeys1 = (from appSettingDto in appSettingDtos
                //                   where appSettingDto.ConfigType == 1
                //                   select appSettingDto.ConfigKey).ToList();
                ////删除 connectionString节点
                //var connectionStringdiffs = GetDiff(localKeys1, remoteKeys1);
                //foreach (var connectionStringdiff in connectionStringdiffs)
                //{
                //    config.ConnectionStrings.ConnectionStrings.Remove(connectionStringdiff);
                //}

                config.Save(ConfigurationSaveMode.Minimal);
                ConfigurationManager.RefreshSection("appSettings");
                ConfigurationManager.RefreshSection("connectionStrings");
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static List<string> GetDiff(List<string> localKeys, List<string> remoteKeys)
        {
            return localKeys.Where(localKey => !remoteKeys.Contains(localKey)).ToList();
        }
    }
}