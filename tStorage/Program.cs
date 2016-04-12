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
            
            tstorage.Open("test1");
            //tstorage.Update("system/query_delim", new char[]{'*'});
            /* */
            tstorage.Create("root/key0", "test", 10);
            tstorage.Create("root/key1", "test");
            tstorage.Commit();
            //*/
            tstorage.Update("root/key0", "test");
            //tstorage.Update("root/key1", "test. OLD entry");
            var ddd = tstorage.Read(new string[] { "root/key0", "root/key1" });

            s.Start(); //start timer

            //for (int i = 0; i < 100000;i++ )
            //{ tstorage.Create("root/key0/sub" + i, "test"); }
            //tstorage.Commit();

            s.Stop(); //stop timer

            Console.WriteLine("msec = {0} // ticks = {1}", s.ElapsedMilliseconds, s.ElapsedTicks);
            Console.ReadLine();
        }
    }
}
