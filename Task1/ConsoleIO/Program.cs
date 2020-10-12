using NetAutumnClassLibrary;

using System.Linq;
using System.Threading;
using System;

namespace ConsoleIO
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Count() == 0)
                return;

            ConcurrentImageProcessor concurrentImageProcessor = new ConcurrentImageProcessor(args[0]);
            
            Console.CancelKeyPress += (sender, eArgs) =>
            {
                concurrentImageProcessor.isStopped.Set();
                eArgs.Cancel = true;
            };

            var printing = new Thread(new ThreadStart(() =>
            {
                Console.WriteLine("Printing has been started.");
                string info = "";
                for (int i = 0; ;)
                {
                    if (concurrentImageProcessor.isStopped.WaitOne(0))
                    {
                        Console.WriteLine("Printing has been finished.");
                        break;
                    }
                    if ((info = concurrentImageProcessor.GetInfo()) != "")
                        Console.WriteLine((++i) + " " + info);
                }

            }));

            printing.Start();
            concurrentImageProcessor.Work();

            printing.Join();

            return;

        }
    }
}
