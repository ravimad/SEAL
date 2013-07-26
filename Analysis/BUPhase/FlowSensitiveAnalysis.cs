using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;
using SafetyAnalysis.Framework.Graphs;
using QuickGraph.Collections;
using Phx.Graphs;

namespace SafetyAnalysis.Purity
{
    using WorklistEntry = Pair<uint, OPER>;

    public enum OPER { PUSH, JOIN };
    public class FlowSensitiveAnalysis
    {
        private  const int HighPriority = 0;
        private const int LowPriority = 1;        
                
        private PurityAnalysisTransformers _transformers = new PurityAnalysisTransformers();        

        public PurityAnalysisData Compute(Phx.FunctionUnit functionUnit)
        {
            var newData = new PartitionPurityData(PartitionGraph.New());
            var endData = CFGFixPointComputation(functionUnit, newData);
            if (endData == null)
                return new PurityAnalysisData(new HeapGraph());
            else 
                return endData.ConvertToPlainData();
        }

        //for now the transformers only perform weak updates, need to modify this
        public PartitionPurityData CFGFixPointComputation(Phx.FunctionUnit functionUnit, PartitionPurityData initData)
        {
            var cfg = functionUnit.FlowGraph;
            //var filename = GeneralUtil.ConvertToFilename(functionUnit);
            //functionUnit.FlowGraph.DumpDot(filename + ".dot", "");
           
            //create a intra procedural worklist of Basic block, prior data pairs.            
            var worklist = new BinaryHeap<int,WorklistEntry>((int x, int y) => (x - y));
            var currentEntries = new HashSet<WorklistEntry>();

            //initialize worklist
            var initEntry = new WorklistEntry(cfg.StartBlock.Id,OPER.PUSH);
            worklist.Add(HighPriority, initEntry);
            currentEntries.Add(initEntry);

            //map basicblocks ids to values.
            //Console.WriteLine("MaxNodeId: " + cfg.MaxNodeId);       
            var inFacts = new PartitionPurityData[cfg.MaxNodeId + 1];
            var outFacts = new PartitionPurityData[cfg.MaxNodeId + 1];
            var outFactChange = new bool[cfg.MaxNodeId + 1];

            inFacts[cfg.StartBlock.Id] = initData;
            
            //Fixpoint iteration
            while (worklist.Any())
            {
                var entry = worklist.RemoveMinimum().Value;
                currentEntries.Remove(entry);

                var bbid = entry.Key;
                var bb = cfg.Block(bbid);                
                var oper = entry.Value;

                if (oper == OPER.PUSH)
                {
                    //if(MethodLevelAnalysis.debug_flag)
                    //    Console.WriteLine("Processing BB: " + bbid);                    
                    //inData will be non-null in this case
                    var inData = inFacts[bbid];                          
                    var outData = EvaluateBlock(bb, inData);

                    //if (MethodLevelAnalysis.debug_flag)
                    //    Console.WriteLine("outData: hasTheVariable ? " + AnalysisUtil.HasVar(outData,7310));                    

                    //check if the outData has changed
                    var oldData = outFacts[bbid];
                    if (HasFactsChanged(outData,oldData))
                    {
                        outFacts[bbid] = outData;                          
                        outFactChange[bbid] = true;
                        foreach (var succEdge in bb.SuccessorEdges)
                        {
                            var succbb = succEdge.SuccessorNode;
                            var newentry = new WorklistEntry(succbb.Id,OPER.JOIN);
                            if(!currentEntries.Contains(newentry))
                            {
                                //using low priority for nodes with many predecesor.
                                if(succbb.PredecessorCount == 1)
                                    worklist.Add(HighPriority, newentry);
                                else 
                                    worklist.Add(LowPriority, newentry);

                                currentEntries.Add(newentry);
                            }
                        }                      
                    }
                }
                else if (oper == OPER.JOIN)
                {
                    //if (MethodLevelAnalysis.debug_flag)
                    //    Console.WriteLine("Joining pred data: " + bbid);
                    var oldData = inFacts[bbid];

                    PartitionPurityData joinData = null;
                    if (oldData != null)
                        joinData = oldData.CopyNonShared();

                    foreach (var predEdge in bb.PredecessorEdges)
                    {
                        var predbb = predEdge.PredecessorNode;
                        if (outFactChange[predbb.Id])
                        {
                            if (bb.PredecessorCount == 1)
                                joinData = outFacts[predbb.Id];
                            else if (joinData == null)
                                joinData = outFacts[predbb.Id].CopyNonShared();
                            else
                            {
                                //assuming that only the non-shared part is copied
                                //union only the unique portions
                                joinData.UnionNonShared(outFacts[predbb.Id]);
                            }
                        }
                    }

                    //if (MethodLevelAnalysis.debug_flag)
                    //    Console.WriteLine("joinData: hasTheVariable ? " + AnalysisUtil.HasVar(joinData, 7310));                    
                                   
                    //check if the inData has changed                    
                    if (HasFactsChanged(joinData, oldData))
                    {
                        inFacts[bbid] = joinData;

                        var newentry = new WorklistEntry(bbid, OPER.PUSH);
                        if (!currentEntries.Contains(newentry))
                        {
                            worklist.Add(HighPriority, newentry);
                            currentEntries.Add(newentry);
                        }
                    }
                }                
            }
            return outFacts[cfg.EndBlock.Id];
        }

        private bool HasFactsChanged(PartitionPurityData newData, PartitionPurityData oldData)
        {
            return (oldData == null 
                || newData.SharedVersion != oldData.SharedVersion 
                || !newData.NonSharedEquivalent(oldData));
        }

        public PartitionPurityData EvaluateBlock(
            Phx.Graphs.BasicBlock block,
            PartitionPurityData inData)
        {
            //need to copy data here
            var outData = inData.CopyNonShared();
            foreach (Phx.IR.Instruction instruction in block.Instructions)
            {
                //Console.WriteLine(instruction);
                _transformers.Apply(instruction, outData);
            }
            return outData;
        }
    }
}
