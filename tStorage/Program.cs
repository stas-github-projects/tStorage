﻿using System;
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

            s.Start(); //start timer

            tstorage.Create("root/key1","test");
            tstorage.Create("root/key2");
            tstorage.Commit();

            s.Stop(); //stop timer

            Console.WriteLine("msec = {0} // ticks = {1}",s.ElapsedMilliseconds,s.ElapsedTicks);
            Console.ReadLine();
        }
    }
}
