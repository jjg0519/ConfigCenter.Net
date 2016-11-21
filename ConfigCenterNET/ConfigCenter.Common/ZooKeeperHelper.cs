using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using ZKClientNET.Client;
using ZKClientNET.Serialize;
using ZooKeeperNet;

namespace ConfigCenter.Common
{
    public class ZooKeeperHelper
    {
        private static readonly string ZookeeperAddress = ConfigurationManager.AppSettings["ZookeeperAddress"];

        private static ZKClient zkClient = ZKClientBuilder.NewZKClient(ZookeeperAddress)
                             .SessionTimeout(10000)
                             .ConnectionTimeout(10000)
                             .Serializer(new SerializableSerializer())
                             .Build();

        public static string ZooKeeperRootNode
        {
            get { return ConfigurationManager.AppSettings["ZookeeperRootNode"]; }
        }

        public static void SetData(string path, string data, int version)
        {
            var nodeData = data == null ? null : data.GetBytes();
            zkClient.WriteData(path, nodeData, version);
        }

        public static byte[] GetData(string path)
        {
            var data = zkClient.ReadData<byte[]>(path);
            return data;
        }

        public static void Create(string path, string data)
        {
            var nodeData = data == null ? null : data.GetBytes();
            zkClient.Create(path, nodeData, Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
        }

        public static void Delete(string path, int version)
        {
            zkClient.Delete(path, version);
        }

        public static List<string> GetChildren(string path)
        {
            var nodepath = zkClient.GetChildren(path).ToList();
            return nodepath;
        }

        public static bool Exists(string path)
        {
            return zkClient.Exists(path, false);
        }
    }
}