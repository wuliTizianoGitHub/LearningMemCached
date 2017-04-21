using Enyim.Caching;
using Enyim.Caching.Memcached;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningMemcached
{
    class Program
    {
        static void Main(string[] args)
        {
            MemcachedClient client = MemcachedManager.GetInstance();
            //client.Store(StoreMode.Set,"key1","value1");
            //MemcachedClient client = MemcachedManager.GetInstance();

            for (int i = 0; i < 1000000; i++)
            {
                client.Store(StoreMode.Set, "key" + i, "value" + i);

                //clien("key" + i, "value" + i, DateTime.Now.AddDays(1));
            }

            var wc=  new Stopwatch();
            wc.Start();


            //object str = client.Get("key500000");


            //client.FlushAll();
            //for (int i = 0; i < 1000000; i++)
            //{
            //    client.Remove("key" + i);
            //}


            List<string>  list = MemcachedManager.GetAllKeys("127.0.0.1",11211);
            
            //foreach (var item in list)
            //{
            //    Console.WriteLine(item);
            //}

            Console.WriteLine(list.Count);
            wc.Stop();

         
            //Console.WriteLine(MemcachedHelper.Get("123"));
            Console.WriteLine(wc.ElapsedMilliseconds);
            Console.ReadKey();
        }
    }
}
