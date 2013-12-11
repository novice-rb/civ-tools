using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEngine
{
    public class PrioQueue<valueType, prioType>
    {
        private int _TotalSize;
        SortedDictionary<prioType, Queue<valueType>> _Storage;

        public PrioQueue()
        {
            this._Storage = new SortedDictionary<prioType, Queue<valueType>>();
            this._TotalSize = 0;
        }

        public bool IsEmpty()
        {
            return (_TotalSize == 0);
        }

        public valueType Dequeue()
        {
            if (IsEmpty())
            {
                throw new Exception("Please check that priorityQueue is not empty before dequeing");
            }
            else
                foreach (var q in _Storage.Values)
                {
                    // we use a sorted dictionary                     
                    if (q.Count > 0)
                    {
                        _TotalSize--;
                        return q.Dequeue();
                    }
                }
            System.Diagnostics.Debug.Assert(false, "not supposed to reach here. problem with changing total_size");
            return default(valueType);
            // not supposed to reach here.         
        }

        // same as above, except for peek.          
        public valueType Peek()
        {
            if (IsEmpty())
                throw new Exception("Please check that priorityQueue is not empty before dequeing");
            else
            {
                foreach (var q in _Storage.Values)
                {
                    if (q.Count > 0) return q.Peek();
                }
                System.Diagnostics.Debug.Assert(false, "not supposed to reach here. problem with changing total_size");
                return default(valueType);
            }
            // not supposed to reach here.
        }

        public valueType Dequeue(prioType prio)
        {
            _TotalSize--;
            return _Storage[prio].Dequeue();
        }

        public void Enqueue(valueType item, prioType prio)
        {
            if (!_Storage.ContainsKey(prio))
            {
                _Storage.Add(prio, new Queue<valueType>());
                Enqueue(item, prio);
                // run again              
            }
            else { _Storage[prio].Enqueue(item); _TotalSize++; }
        }
    }
}
