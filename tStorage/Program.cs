using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace tStorage
{
    class Program
    {
        static void Main(string[] args)
        {
            GC.Collect(0, GCCollectionMode.Optimized); //force gc on 0 level & gcserver = enbled in 'app.config'

            Stopwatch s = new Stopwatch();
            //s.Start();

            tStorage tstorage = new tStorage();

            //start timer
            s.Start(); 

            tstorage.Open("test1");
            //tstorage.Update("system/query_delim", new char[]{'*'});
            /* /
            tstorage.Create("root/key0", "test1", 10);
            tstorage.Create("root/key1", "test2", 22);
            tstorage.Create("root/key2", "test3");
            /* /
            tstorage.Create("root/s/key0", "test1", 10);
            tstorage.Create("root/s/key1", "test2", 22);
            tstorage.Create("root/s/key2", "test3");
            //*/
            //tstorage.Create("root/s2/key0", "test1", 10);
            //tstorage.Create("root/s2/key1", "test2", 22);
            //tstorage.Create("root/s2/key2", "test3");
            //tstorage.Commit();
            //*/
            //tstorage.Create("root/key3", "testov");
            //tstorage.Commit();
            //tstorage.Update("root/key0", "test1");
            //tstorage.Update("root/key1", "test. OLD entry");

            //tstorage.Delete("root/key1");

            List<dynamic> lst_out = new List<dynamic>(100);
            for (int i = 0; i < 1; i++)
            {
                lst_out.Add(tstorage.ReadWKey(new string[] { "root/*/key1" }));
            }

            //for (int i = 0; i < 100000;i++ )
            //{ tstorage.Create("root/key0/sub" + i, "test"); }
            //tstorage.Commit();

            s.Stop(); //stop timer

            Console.WriteLine("msec = {0} // ticks = {1}", s.ElapsedMilliseconds, s.ElapsedTicks);
            Console.ReadLine();
        }
    }
}
