using Memcached.ClientLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningMemcached
{
    public static class MemcachedHelper
    {
        private static MemcachedClient mclient;
           
        /// <summary>
        /// 
        /// </summary>
        static MemcachedHelper()
        {
            //服务器列表
            string[] serverList = new string[] { "127.0.0.1:11211" };
            //IO池
            SockIOPool pool = SockIOPool.GetInstance("First");

            //设置服务器
            pool.SetServers(serverList);

            //初始化
            pool.Initialize();

            mclient = new MemcachedClient();

            mclient.PoolName = "First";

            mclient.EnableCompression = false;
        }

        public static bool Set(string key,object value,DateTime expiry)
        {
            return mclient.Set(key, value, expiry);
        }

        public static object Get(string key)
        {
            return mclient.Get(key);
        }
    }
}
