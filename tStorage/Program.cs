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
            GC.Collect(2, GCCollectionMode.Optimized);

            Stopwatch s = new Stopwatch();
            //s.Start();

            tStorage tstorage = new tStorage();
            
            tstorage.Open("test1");

            

            s.Start();
            for (int i = 0; i < 1000000; i++)
            { tstorage.Create("root/key"+i); }

            s.Stop();
            Console.WriteLine("msec = {0} // ticks = {1}",s.ElapsedMilliseconds,s.ElapsedTicks);
            Console.ReadLine();
        }
    }
}
