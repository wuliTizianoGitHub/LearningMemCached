using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LearningMemcached
{
    public static class MemcachedManager
    {
        private static MemcachedClient client;

        private static readonly object memcachelock = new object();

        /// <summary>
        /// 以线程安全的方式获取<see cref="MemcachedClient"/>的实例
        /// </summary>
        /// <returns></returns>
        public static MemcachedClient GetInstance()
        {
            if (client==null)
            {
                lock (memcachelock)
                {
                    if (client == null)
                    {
                        ClientInit();
                    }
                }
            }
            return client;
        }

        /// <summary>
        /// 初始化<see cref="MemcachedClient"/>对象
        /// </summary>
        static void ClientInit()
        {
            try
            {
                client = new MemcachedClient("enyim.com/memcached");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static MemcachedManager()
        {
            GetInstance();
        }


        public static void Store(string Key, object Value, DateTime ExpiredAt)
        {
            client.Store(StoreMode.Set, Key, Value, ExpiredAt);
        }

        public static T Get<T>(string Key)
        {
            return client.Get<T>(Key);
        }

        public static void Remove(string Key)
        {
            client.Remove(Key);
        }



        public static List<string> GetAllKeys(string ipString, int port)
        {
            List<string> allKeys = new List<string>();
            //var ipString = "127.0.0.1";
            //var port = 11211;

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Parse(ipString), port));
            var slabIdIter = QuerySlabId(socket);
            var keyIter = QueryKeys(socket, slabIdIter);
            socket.Close();

            foreach (string key in keyIter)
            {
                if (!allKeys.Contains(key))
                    allKeys.Add(key);
            }

            return allKeys;
        }

        /// <summary>
        /// 执行返回字符串标量
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="command">命令</param>
        /// <returns>执行结果</returns>
        static string ExecuteScalarAsString(Socket socket, string command)
        {
            var sendNumOfBytes = socket.Send(Encoding.UTF8.GetBytes(command));
            var bufferSize = 0x1000;
            var buffer = new Byte[bufferSize];
            var readNumOfBytes = 0;
            var sb = new StringBuilder();

            while (true)
            {
                readNumOfBytes = socket.Receive(buffer);
                sb.Append(Encoding.UTF8.GetString(buffer));

                if (readNumOfBytes < bufferSize)
                    break;
            }

            return sb.ToString();
        }

        /// <summary>
        /// 查询slabId
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <returns>slabId遍历器</returns>
        static IEnumerable<string> QuerySlabId(Socket socket)
        {
            var command = "stats items STAT items:0:number 0 \r\n";
            var contentAsString = ExecuteScalarAsString(socket, command);

            return ParseStatsItems(contentAsString);
        }

        /// <summary>
        /// 解析STAT items返回slabId
        /// </summary>
        /// <param name="contentAsString">解析内容</param>
        /// <returns>slabId遍历器</returns>
        static IEnumerable<string> ParseStatsItems(string contentAsString)
        {
            var slabIds = new List<string>();
            var separator = "\r\n";
            var separator2 = ':';
            var items = contentAsString.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < items.Length; i += 4)
            {
                var itemParts = items[i].Split(separator2, StringSplitOptions.RemoveEmptyEntries);

                if (itemParts.Length < 3)
                    continue;

                slabIds.Add(itemParts[1]);
            }

            return slabIds;
        }

        /// <summary>
        /// 查询键
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="slabIdIter">被查询slabId</param>
        /// <returns>键遍历器</returns>
        static IEnumerable<string> QueryKeys(Socket socket, IEnumerable<string> slabIdIter)
        {
            var keys = new List<string>();
            var cmdFmt = "stats cachedump {0} 200000 ITEM views.decorators.cache.cache_header..cc7d9 [6 b; 1256056128 s] \r\n";
            var contentAsString = string.Empty;

            foreach (string slabId in slabIdIter)
            {
                contentAsString = ExecuteScalarAsString(socket, string.Format(cmdFmt, slabId));
                keys.AddRange(ParseKeys(contentAsString));
            }

            return keys;
        }

        /// <summary>
        /// 解析stats cachedump返回键
        /// </summary>
        /// <param name="contentAsString">解析内容</param>
        /// <returns>键遍历器</returns>
        static IEnumerable<string> ParseKeys(string contentAsString)
        {
            var keys = new List<string>();
            var separator = "\r\n";
            var separator2 = ' ';
            var prefix = "ITEM";
            var items = contentAsString.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in items)
            {
                var itemParts = item.Split(separator2, StringSplitOptions.RemoveEmptyEntries);

                if ((itemParts.Length < 3) || !string.Equals(itemParts.FirstOrDefault(), prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                keys.Add(itemParts[1]);
            }

            return keys;
        }
    }

    /// <summary>
    /// String扩展函数
    /// </summary>
    static class StringExtension
    {
        /// <summary>
        /// 切割
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="separator">分隔符</param>
        /// <param name="options">选项</param>
        /// <returns>切割结果</returns>
        public static string[] Split(this string str, char separator, StringSplitOptions options)
        {
            return str.Split(new char[] { separator }, options);
        }

        /// <summary>
        /// 切割
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="separator">分隔符</param>
        /// <param name="options">选项</param>
        /// <returns>切割结果</returns>
        public static string[] Split(this string str, string separator, StringSplitOptions options)
        {
            return str.Split(new string[] { separator }, options);
        }
    }
}
