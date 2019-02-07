using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OperatingSystemSJFAlgorithm
{
    public class SJF
    {
        // Process ready to work
        List<CustomProcess> readyProcess;
        // Process waiting his turn
        List<CustomProcess> waitingProcess;
        // Process has worked all his time
        List<CustomProcess> workedProcess;
        // Lockable object - it's only one for all SJF
        public static object locker = new object();
        // Amount of RAM - v and HDD - h
        int v = 15, h = 14;
        public static EventWaitHandle mainWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        // Current tact number and index of current process
        int tacts = 0, index = -1;
        // T0 for non preemptive planning with execution prehistory
        public static int t0 = 5;
        // Which of SJF variation currently running
        public static Method SJFMethod;
        public enum Method
        {
            RemainingExecutionTime, // Preemptive planning for remaining execution time
            WithExecutionPrehistory // Non preemptive planning with execution prehistory
        }

        public SJF()
        {
            readyProcess = new List<CustomProcess>();
            waitingProcess = new List<CustomProcess>();
            workedProcess = new List<CustomProcess>();
        }

        public SJF(IEnumerable<CustomProcess> processes)
        {
            readyProcess = new List<CustomProcess>();
            waitingProcess = new List<CustomProcess>();
            workedProcess = new List<CustomProcess>();
            waitingProcess.AddRange(processes);
            // Sorting waiting process by arrival time (in which tact it appeared in system) 
            // or name if arrival times of processes is the same
            waitingProcess.Sort(delegate (CustomProcess first, CustomProcess second)
            {
                if (first.ArrivalTime < second.ArrivalTime) return -1;
                else if (first.ArrivalTime > second.ArrivalTime) return 1;
                else
                {
                    return first.Name.CompareTo(second.Name);
                }
            });
        }

        // Adding process to waiting list
        public void AddProcess(CustomProcess process)
        {
            // Checking if other methods don't change list data and locking object to prevent this
            lock (locker)
            {
                waitingProcess.Add(process);
            }
        }

        // Method that called outside to running SJF algorithm
        public async Task<List<CustomProcess>> Start()
        {
            // Create task and waiting for result
            return await Task.Run(() => Work());
        }

        // Main method implements two SJF algorithm
        private List<CustomProcess> Work()
        {
            // Waiting process go to ready process if system has free amount of RAM and HDD for that process
            CheckResources();
            while (waitingProcess.Count > 0 || readyProcess.Count > 0)
            {
                // Checking if other methods don't change list data and locking object to prevent this
                lock (locker) { }
                if (readyProcess.Count > 0)
                {
                    // Checking if system have not sleeping ready process
                    if (readyProcess.Where(p => !p.Sleep).Count() > 0)
                    {
                        // Getting index of process in ready list which fulfill requirements
                        if (SJFMethod == Method.RemainingExecutionTime)
                        {
                            int minTime = readyProcess.Where(p => !p.Sleep).Min(p => p.WorkingTime);
                            index = readyProcess.FindIndex(p => p.WorkingTime == minTime && !p.Sleep);
                        }
                        else if (SJFMethod == Method.WithExecutionPrehistory)
                        {
                            if (index == -1 || readyProcess[index].Sleep)
                            {
                                foreach (CustomProcess process in readyProcess.Where(p => !p.Sleep))
                                {
                                    if (process.WorkingTimePredict == 0)
                                        process.WorkingTimePredict = t0;
                                    else
                                        process.WorkingTimePredict = 0.5 * process.Worked + 0.5 * t0;
                                }
                                double minTime = readyProcess.Where(p => !p.Sleep).Min(p => p.WorkingTimePredict);
                                index = readyProcess.FindIndex(p => p.WorkingTimePredict == minTime && !p.Sleep);
                            }
                        }
                        // Run chosen process if it unstarted, else set a wait handle
                        if (readyProcess[index].Task.ThreadState == ThreadState.Unstarted)
                        {
                            readyProcess[index].Task.Start();
                        }
                        else
                        {
                            readyProcess[index].WaitHandle.Set();
                        }
                        // Waiting while running process stop and return control back
                        mainWaitHandle.WaitOne();
                        // When process worked his time completely, return all resources back to system
                        if (readyProcess[index].WorkingTime == 0)
                        {
                            h += readyProcess[index].H;
                            v += readyProcess[index].V;
                            readyProcess[index].TimeOfRun = tacts - readyProcess[index].ArrivalTime + 1;
                            workedProcess.Add(readyProcess[index]);
                            readyProcess.RemoveAt(index);
                            index = -1;
                        }
                    }
                    tacts++;
                    // Adding to sleeping processes one tacts and check if it already awake
                    foreach (CustomProcess process in readyProcess.Where(p => p.Sleep))
                    {
                        if (process.Worked > 0)
                            process.Waited++;
                        if (process.Waited == process.H + 2 || process.Waited == 0)
                        {
                            process.Sleep = false;
                            process.Waited = 0;
                        }
                    }
                }
                // Checking resources if system have free amount of RAM and HDD
                if (v > 0 && h > 0)
                    CheckResources();
            }
            // After all process worked up, writing to console number of tacts
            Console.WriteLine("\nNumber of tacts: " + tacts);
            return workedProcess;
        }

        private void CheckResources()
        {
            // Checking if other methods don't change list data and locking object to prevent this
            lock (locker)
            {
                // Iterating over list from first to last bcos it's already sorted by arrival time
                for (int i = 0, n = waitingProcess.Count(); i < n; i++)
                {
                    // Check if system has free amount of RAM, HDD for process and it's already arrived
                    if (waitingProcess[i].V <= v && waitingProcess[i].H <= h && tacts >= waitingProcess[i].ArrivalTime)
                    {
                        // Consuming RAM and HDD from system
                        v -= waitingProcess[i].V;
                        h -= waitingProcess[i].H;
                        // If process already arrived in system, then it in sleep mode
                        if (tacts > waitingProcess[i].ArrivalTime)
                            waitingProcess[i].Sleep = true;
                        // Waiting process changing it state to ready
                        readyProcess.Add(waitingProcess[i]);
                        waitingProcess.RemoveAt(i);
                        // Changing values of i and n that system don't go outside of list
                        i--;
                        n--;
                    }
                }
            }
        }
    }
}
