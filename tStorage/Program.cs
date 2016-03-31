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
            Stopwatch s = new Stopwatch();
            s.Start();



            s.Stop();
            Console.WriteLine("msec = {0} // ticks = {1}",s.ElapsedMilliseconds,s.ElapsedTicks);
            Console.ReadLine();
        }
    }
}
