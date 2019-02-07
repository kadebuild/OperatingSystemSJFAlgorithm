using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OperatingSystemSJFAlgorithm
{
    class Program
    {
        static void Main(string[] args)
        {
            List<CustomProcess> newProcess = new List<CustomProcess>();
            ConsoleKeyInfo keypress;
            // Menu of console application
            Console.WriteLine("1 - SJF Remaining Execution Time / 2 - SJF With Execution Prehistory T0 = 5");
            Console.WriteLine("3 - Add new process / 0 - Exit");
            do
            {
                keypress = Console.ReadKey();
                if (keypress.KeyChar == '1')
                    StartSJFProcess(SJF.Method.RemainingExecutionTime, newProcess);
                else if (keypress.KeyChar == '2')
                    StartSJFProcess(SJF.Method.WithExecutionPrehistory, newProcess);
                else if (keypress.KeyChar == '3')
                {
                    Console.WriteLine("Enter new process, all data entered through a space");
                    Console.WriteLine("Name V H N WorkingTime ArrivalTime");
                    string buf = Console.ReadLine();
                    string[] buf2 = buf.Split(' ');
                    newProcess.Add(new CustomProcess(buf2[0], Convert.ToInt32(buf2[1]), Convert.ToInt32(buf2[2]), Convert.ToInt32(buf2[3]), Convert.ToInt32(buf2[4]), Convert.ToInt32(buf2[5])));
                }
            } while (keypress.KeyChar != '0');
            Console.WriteLine("Press any key");
            Console.ReadKey();
        }

        public static void StartSJFProcess(SJF.Method method, List<CustomProcess> newProcess)
        {
            // Create list of process
            List<CustomProcess> processList = new List<CustomProcess>()
            {
                new CustomProcess("P0", 3, 4, 7, 15, 1),
                new CustomProcess("P1", 7, 4, 2, 3, 0),
                new CustomProcess("P2", 7, 4, 2, 3, 1),
                new CustomProcess("P3", 2, 3, 3, 6, 2),
                new CustomProcess("P4", 2, 3, 3, 6, 1),
                new CustomProcess("P5", 1, 3, 4, 6, 4),
                new CustomProcess("P6", 5, 3, 3, 4, 1),
                new CustomProcess("P7", 1, 3, 4, 6, 3),
                new CustomProcess("P8", 9, 1, 2, 4, 3),
                new CustomProcess("P9", 2, 3, 3, 6, 3)
            };

            // If we choose to add another process, then adding it to end of process list
            if (newProcess.Count() > 0)
            {
                foreach (CustomProcess process in newProcess)
                {
                    processList.Add(process);
                }
            }
            SJF sjf = new SJF(processList);
            SJF.SJFMethod = method; // SJF вытесняющий
            Console.WriteLine("SJF " + method + ": ");
            // Starting SJF process and waiting for results
            Task<List<CustomProcess>> result = sjf.Start();
            result.Wait();
            // When task is finished - calculate average time of running and waiting
            Console.WriteLine("Average running time: " + result.Result.Sum(p => p.TimeOfRun) / result.Result.Count());
            Console.WriteLine("Average waiting time: " + (result.Result.Sum(p => p.TimeOfRun) - result.Result.Sum(p => p.Worked)) / result.Result.Count());
        }
    }
}
