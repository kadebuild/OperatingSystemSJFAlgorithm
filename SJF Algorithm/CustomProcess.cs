using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OperatingSystemSJFAlgorithm
{
    public class CustomProcess
    {
        // Process name
        public string Name { get; private set; }
        // Consumption of RAM
        public int V { get; private set; }
        // Consumption of HDD
        public int H { get; private set; }
        // Number of tacts that process need to worked up before sleep
        public int N { get; private set; }
        // Working time - how much tacts process need to work before finish
        public int WorkingTime { get; private set; }
        // Arrival time - in which tact process came to system
        public int ArrivalTime { get; private set; }
        // Process which needed to run
        public Thread Task { get; private set; }
        public EventWaitHandle WaitHandle { get; private set; }
        // How much tacts process worked up
        public int Worked { get; private set; }
        // How much tacts process sleeps
        public int Waited { get; set; }
        // Sleeps right now?
        public bool Sleep { get; set; }
        // Predicting how much tacts process will need to finish (only for SJF with execution prehistory)
        public double WorkingTimePredict { get; set; }
        // How much tacts process rans before finish (running time + waiting time)
        public double TimeOfRun { get; set; }

        public CustomProcess(string name, int v, int h, int n, int workingTime, int arrivalTime)
        {
            Name = name;
            V = v;
            H = h;
            N = n;
            WorkingTime = workingTime;
            ArrivalTime = arrivalTime;
            Task = new Thread(RunProcess);
            WaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            Worked = 0;
            Waited = 0;
            Sleep = false;
            WorkingTimePredict = 0;
        }

        public void RunProcess()
        {
            while (true)
            {
                // Checking if process can start (continues) to work and lock object to prevent other process running
                lock (SJF.locker)
                {
                    // Working one tact and write name of process to console
                    WorkingTime--;
                    Worked++;
                    Console.Write(Name + " ");
                    // Checking if process worked up N tacts and not finished
                    if (Worked % N == 0 && WorkingTime > 0)
                    {
                        Sleep = true;
                        Console.Write("I/O ");
                        // Predicting how much tacts process will need to finish
                        WorkingTimePredict = 0.5 * Worked + 0.5 * SJF.t0;
                    }
                }
                // Return control to system
                SJF.mainWaitHandle.Set();
                // If process worked up all tacts, then stop it
                if (WorkingTime <= 0)
                {
                    WaitHandle.Close();
                    return;
                }
                // Else waiting when system give control back to that process
                WaitHandle.WaitOne();
            }
        }
    }
}
