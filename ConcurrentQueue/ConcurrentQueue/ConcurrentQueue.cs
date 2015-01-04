using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentQueue
{
    public class ConcurrentQueue<T>
    {
        private class Node<T>
        {
            internal T m_value;
            internal Node<T> m_next;

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

        private volatile Node<T> m_head;
        private volatile Node<T> m_tail;

        public ConcurrentQueue()
        {
            m_head = new Node<T>();
            m_tail = m_head;
        }

/* Visual Studio gives a warning when we try use ref with a volatile variable.
 * However, it is safe to do so in case of the Interlocked family of functions
 * according to MSDN. */
#pragma warning disable 0420
        public void Enqueue(T val)
        {
            Node<T> newNode = new Node<T>(val);
            Node<T> localTail;

            /* Use a while loop to maintain the following condition after it exits:
             * m_tail.next = newNode
             * We will attempt to update the m_tail to newNode after the while loop.
             * It is also possible that the update won't happen at the end of while loop.
             * However, that will only be possible if another thread has already updated m_tail
             * to the correct address. */
            while (true)
            {
                localTail = m_tail;
                Node<T> localTailNext = m_tail.m_next;

                /* Make a quick check to see if m_tail has already been updated */
                if(localTail != m_tail)
                    continue;

                /* localTailNext may not be null in case a thread was suspended any time before the last
                 * CompareExchange outside of this loop and after the last CompareExchange within the loop
                 * and another Enqueue operation scheduled after that successfully updated m_tail.
                 * If that is the case, we need to update m_tail and start this loop all over. */
                if (null != localTailNext)
                {
                    Interlocked.CompareExchange < Node<T>>(ref m_tail, localTailNext, localTail);
                    continue;
                }

                /* Hopefully m_tail.m_next is null. 
                 * If yes, we make it point to newNode.
                 * If not, we need to start this loop all over.
                 * This can happen if a thread got suspended after the check of localTailNext equal to null
                 * and soon after that another Enqueue operation ran to completion. */
                if(null == Interlocked.CompareExchange < Node<T>>(ref m_tail.m_next, newNode, null))
                    break;
            }
            
            /* Make an attempt to fix m_tail to make it equal to newNode.
             * However, we need not worry if it fails. This is because it can only mean 
             * that another thread has already updated m_tail to the correct value in:
             * Interlocked.CompareExchange(ref m_tail, localTailNext, localTail); above. */
            Interlocked.CompareExchange < Node<T>>(ref m_tail, newNode, localTail);
        }

        public bool TryPeekAndDequeue(out T retValue)
        {
            Node<T> localHead;
            Node<T> localTail;
            Node<T> localHeadNext;
            T valToReturn;
            while (true)
            {
                localHead = m_head;
                localTail = m_tail;
                localHeadNext = localHead.m_next;

                /* Make a quick check to see if m_head has already been updated. */
                if(localHead != m_head)
                    continue;

                /* we can be sure there is no element in the queue only if:
                 * 1.) head and tail refer to the same node.
                 * 2.) next of head is null.
                 * The reason why 2.) is important is there could be a thread which might be executing an Enqueue.
                 * If that thread is in the while loop of the Enqueue operation or has not yet updated m_tail
                 * to refer to the new node, although head and tail both point to the same node, yet the next
                 * is not null (It is equal to the new node just added.)
                 * In that case, we need to try and update m_tail (It could be possible that another thread has
                 * already updated m_tail so if it fails here, we need not worry about that.) */
                if (localHead == localTail)
                {
                    if (null == localHeadNext)
                    {
                        retValue = default(T);
                        return false;
                    }

                    /* Oops, somebody actually enqueued an item and may not yet have updated m_tail.
                     * Let us try to update m_tail and start the loop all over. */
                    Interlocked.CompareExchange < Node<T>>(ref m_tail, localHeadNext, localTail);
                    continue;
                }
                valToReturn = localHeadNext.m_value;

                /* Check if m_head is the same as the value we cached. If it is, we update m_head to point to 
                 * its next node and break out of the loop. */
                if (localHead == Interlocked.CompareExchange < Node<T>>(ref m_head, localHeadNext, localHead))
                    break;
            }
            retValue = valToReturn;
            return true;
        }
        public bool TryPeek(out T retValue)
        {
            if (m_head == m_tail)
            {
                retValue = default(T);
                return false;
            }
            retValue = m_head.m_next.m_value;
            return true;
        }
    }
}
