using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UpdateService
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                StartUpdateService();
            }
            catch (Exception exception)
            {
                Console.WriteLine("error {0}", exception);
            }
        }

        public static void StartUpdateService()
        {
            Console.WriteLine("starting");
            new DllUpdateProcess(true).Start();
            Thread.Sleep(100);
        }
    }
}
