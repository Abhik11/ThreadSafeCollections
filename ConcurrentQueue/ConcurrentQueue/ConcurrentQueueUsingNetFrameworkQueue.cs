using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentQueue
{
    class ConcurrentQueueUsingNetFrameworkQueue<T>
    {
        private Queue<T> q;
        private readonly object locker;

        public ConcurrentQueueUsingNetFrameworkQueue()
        {
            q = new Queue<T>();
            locker=new object();
        }
        public void Enqueue(T val)
        {
            lock (locker)
                q.Enqueue(val);
        }

        public bool TryPeekAndDequeue(out T retValue)
        {
            lock (locker)
            {
                if (0 != q.Count)
                {
                    retValue = q.Dequeue();
                    return true;
                }
                retValue = default(T);
                return false;
            }
        }
    }
}
