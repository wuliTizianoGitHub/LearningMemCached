using Enyim.Caching;
using Enyim.Caching.Memcached;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningMemcached
{
    class Program
    {
        static void Main(string[] args)
        {
            //MemcachedClient client = MemcachedManager.GetInstance();
            //client.Store(StoreMode.Set,"key1","value1");
            MemcachedManager.Store("key1", "value1", DateTime.Now.AddDays(1));

            string value = MemcachedManager.Get<string>("key1");
            List<string>  list = MemcachedManager.GetAllKeys("127.0.0.1",11211);
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
            
          

            //MemcachedHelper.Set("123","123456",DateTime.Now.AddDays(1));
            //Console.WriteLine(MemcachedHelper.Get("123"));
            Console.ReadKey();
        }
    }
}
