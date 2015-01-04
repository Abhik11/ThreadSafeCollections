using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentQueue
{
    class ConcurrentQueueUsingLocks<T>
    {
        private class Node
        {
            internal T m_value;
            internal Node m_next;

            internal Node(T mValue)
            {
                m_value = mValue;
                m_next = null;
            }

            internal Node()
            {
                m_value = default(T);
                m_next = null;
            }
        }

        private volatile Node m_head;
        private volatile Node m_tail;
        private object locker;

        public ConcurrentQueueUsingLocks()
        {
            m_head = new Node();
            m_tail = m_head;
            locker = new object();
        }

        public void Enqueue(T val)
        {
            Node newNode = new Node(val);
            lock (locker)
            {
                m_tail.m_next = newNode;
                m_tail = newNode;
            }
        }

        public bool TryPeekAndDequeue(out T retValue)
        {
            lock (locker)
            {
                if (m_head == m_tail)
                {
                    retValue = default(T);
                    return false;
                }
                retValue = m_head.m_next.m_value;
                m_head = m_head.m_next;
                return true;
            }
        }
    }
}
