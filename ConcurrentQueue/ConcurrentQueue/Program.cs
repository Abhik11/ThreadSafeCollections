using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentQueue
{
    class Program
    {
        private static readonly int numValuesToEnqueue = 1000000;
        private static void TestNobBlockingConcurrentQueue()
        {
            int[] valuesPopped = new int[numValuesToEnqueue + 1];
            ConcurrentQueueUsingLocks<int> q = new ConcurrentQueueUsingLocks<int>();
            int poppedValue;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Thread pushThread = new Thread(() =>
            {
                for (int i = 1; i <= numValuesToEnqueue; i++)
                {
                    q.Enqueue(i);
                }
            });
            pushThread.Start();
            Thread popThread = new Thread(() =>
            {
                while (true)
                {
                    bool successInPopping = q.TryPeekAndDequeue(out poppedValue);
                    if (successInPopping)
                        valuesPopped[poppedValue]++;
                    else
                        break;
                }
            });
            popThread.Start();
            pushThread.Join();
            while (true)
            {
                bool successInPopping = q.TryPeekAndDequeue(out poppedValue);
                if (successInPopping)
                    valuesPopped[poppedValue]++;
                else
                    break;
            }
            sw.Stop();
            Console.WriteLine("Time elapsed: " + sw.ElapsedMilliseconds);
            for (int i = 1; i <= numValuesToEnqueue; i++)
            {
                if (0 == valuesPopped[i])
                    Console.WriteLine("\n" + i + " was not popped");
                else if (1 < valuesPopped[i])
                    Console.WriteLine("\n" + i + " was popped twice");
            }
        }

 
        static void Main(string[] args)
        {
            //TestNobBlockingConcurrentQueue();
            ConcurrentQueue<int> q = new ConcurrentQueue<int>();
            int poppedValue;
            new Thread(()=>q.Enqueue(1)).Start();
            Thread.Sleep(1);
            new Thread(() => q.TryPeekAndDequeue(out poppedValue)).Start();
            Thread.Sleep(1000);
            Console.Read();
        }
    }
}
