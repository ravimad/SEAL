//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Diagnostics;
//using System.Diagnostics.Contracts;

//using SafetyAnalysis.Framework.Graphs;
//using SafetyAnalysis.Purity.HandlerProvider;
//using SafetyAnalysis.Purity;

//namespace SafetyAnalysis.Purity.Summaries
//{
//    /// <summary>
//    /// This uses two maps a map (keys-values) and a backward map form (value to keys).
//    /// </summary>
//    public class VertexMultimap : Dictionary<HeapVertexBase, HeapVertexSet>
//    {
//        Dictionary<HeapVertexBase, HeapVertexSet> backwardMap = new Dictionary<HeapVertexBase, HeapVertexSet>();

//        int size = 0;
//        internal VertexMultimap()
//        {
//        }

//        internal bool Contains(HeapVertexBase firstVertex, HeapVertexBase secondVertex)
//        {
//            if (!this.ContainsKey(firstVertex))
//                return false;

//            if (!this[firstVertex].Contains(secondVertex))
//                return false;

//            return true;
//        }

//        internal void Add(HeapVertexBase firstVertex, HeapVertexBase secondVertex)
//        {
//            //update the forward map
//            if (!this.ContainsKey(firstVertex))
//            {
//                this[firstVertex] = new HeapVertexSet();
//                this[firstVertex].Add(secondVertex);
//            }
//            else if (!this[firstVertex].Contains(secondVertex))
//                this[firstVertex].Add(secondVertex);

//            //update the backward map
//            if (!backwardMap.ContainsKey(secondVertex))
//            {
//                backwardMap[secondVertex] = new HeapVertexSet();
//                backwardMap[secondVertex].Add(firstVertex);
//            }
//            else if (!backwardMap[secondVertex].Contains(firstVertex))
//                backwardMap[secondVertex].Add(firstVertex);

//            size++;
//        }

//        internal int getSize()
//        {
//            return size;
//        }

//        internal HeapVertexSet FindKeysForValue(HeapVertexBase secondVertex)
//        {
//            if (backwardMap.ContainsKey(secondVertex))
//                return backwardMap[secondVertex];
//            else return new HeapVertexSet();
//        }        
//    }
//}
