using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Util
{
    public interface IWorklist<T>
    {
        void Enqueue(T e);
        T Dequeue();
        bool Any();
        bool Contains(T e);
        int Count();
    }

    public class LRFWorklist<T> : IWorklist<T>
    {
        int timestamp;
        Dictionary<T, int> TimestampMap;
        HashSet<T> _worklist = new HashSet<T>();    

        public LRFWorklist()
        {
            timestamp = 0;
            TimestampMap = new Dictionary<T, int>();
        }

        public void Enqueue(T e)
        {
            _worklist.Add(e);            
        }

        public int GetTimeStamp(T e)
        {
            int ts;
            if (TimestampMap.TryGetValue(e, out ts))
            {
                return ts;
            }
            return 0;
        }

        public T Dequeue()
        {
            timestamp++;            
            
            //pick the node with lowest timestamp
            T minNode = _worklist.First();
            int minTS = GetTimeStamp(minNode);
            if (minTS != 0)
            {
                foreach (var e in _worklist)
                {
                    int ts = GetTimeStamp(e);
                    if (ts < minTS)
                    {
                        minNode = e;
                        minTS = ts;
                    }
                }
            }
            _worklist.Remove(minNode);

            //update the timestamp of the minNode to the current timestamp
            if (TimestampMap.ContainsKey(minNode))
                TimestampMap.Remove(minNode);
            TimestampMap.Add(minNode, timestamp);

            return minNode;
        }

        public bool Any()
        {
            return _worklist.Any();
        }

        public bool Contains(T e)
        {
            return _worklist.Contains(e);
        }

        public int Count()
        {
            return _worklist.Count;
        }
    }

    public class MRWorklist<T> : IWorklist<T>
    {
        int timestamp;
        QuickGraph.Collections.BinaryHeap<int,T> minheap;
        HashSet<T> worklist;

        public MRWorklist()
        {
            timestamp = 0;
            minheap = new QuickGraph.Collections.BinaryHeap<int, T>();
            worklist = new HashSet<T>();
        }

        public void Enqueue(T e)
        {
            minheap.Add(timestamp--, e);
            worklist.Add(e);
        }
        
        public T Dequeue()
        {
            //return the one with the lowest time stamp which is the most recently added
            var minval = minheap.RemoveMinimum().Value;            
            worklist.Remove(minval);
            return minval;
        }

        public bool Any()
        {
            return worklist.Any();
        }

        public bool Contains(T e)
        {
            return worklist.Contains(e);
        }

        public int Count()
        {
            return worklist.Count;
        }
    }
}
