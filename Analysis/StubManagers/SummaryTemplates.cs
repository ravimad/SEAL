using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Util;
using SafetyAnalysis.Purity.HandlerProvider;

namespace SafetyAnalysis.Purity.Summaries
{
    public class SummaryTemplates
    {        
        //site id used by the allocator
        private static uint internalNodeSiteid = 29;
        private static uint loadNodeSiteid = 31;        

        public static PurityAnalysisData CreatePureData()
        {
            var data = new PurityAnalysisData(new HeapGraph());
            var glv = GlobalLoadVertex.GetInstance();            
            data.OutHeapGraph.AddVertex(glv);

            var retVertex = ReturnVertex.GetInstance();
            data.OutHeapGraph.AddVertex(retVertex);

            var exceptionVertex = ExceptionVertex.GetInstance();
            data.OutHeapGraph.AddVertex(exceptionVertex);
            return data;
        }
               
        /// <summary>
        /// Makes the return value of outData is a newly allocated object
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type">type of the allocated object</param>        
        /// <param name="methodName">name of the containing method</param>
        public static void MakeAllocator(
            PurityAnalysisData outData,
            string type,
            string methodName)
        {
            var id = SummaryTemplates.internalNodeSiteid;

            var newObjectVertex = NodeEquivalenceRelation.CreateInternalHeapVertex(
                methodName, id, Context.EmptyContext);
            if (!outData.OutHeapGraph.ContainsVertex(newObjectVertex))
            {
                outData.OutHeapGraph.AddVertex(newObjectVertex);
                if (type == null)
                    outData.AddConcreteType(newObjectVertex, PurityAnalysisData.AnyType);
                else
                    outData.AddConcreteType(newObjectVertex, type);
                
            }

            var retVertex = ReturnVertex.GetInstance();

            var edge = new InternalHeapEdge(retVertex, newObjectVertex, null);
            if (!outData.OutHeapGraph.ContainsHeapEdge(edge))
                outData.OutHeapGraph.AddEdge(edge);
        }

        /// <summary>
        /// make the return value of outdata point-to an static object
        /// </summary>
        /// <param name="data"></param>
        /// <param name="inst"></param>
        public static void ApplyReturnsStaticObjectSummary(
            PurityAnalysisData outData,            
            Field field,
            string methodName)
        {
            var id = SummaryTemplates.loadNodeSiteid;

            var glv = GlobalLoadVertex.GetInstance();
            
            //create a load heap vertex
            var loadVertex = NodeEquivalenceRelation.CreateLoadHeapVertex(
                methodName, id, Context.EmptyContext);
            if (!outData.OutHeapGraph.ContainsVertex(loadVertex))
                outData.OutHeapGraph.AddVertex(loadVertex);

            //add an external edge from glvto load vertex
            var edge = new ExternalHeapEdge(glv, loadVertex, field);
            if (!outData.OutHeapGraph.ContainsHeapEdge(edge))
                outData.OutHeapGraph.AddEdge(edge);

            //add a return vertex
            var retVertex = ReturnVertex.GetInstance();
            
            //add an internal edge from ret vertex to the loadVertex
            var retedge = new InternalHeapEdge(retVertex, loadVertex, null);
            if (!outData.OutHeapGraph.ContainsHeapEdge(retedge))
                outData.OutHeapGraph.AddEdge(edge);            
        }

        public static void WriteGlobalObject(PurityAnalysisData data, Field field)
        {
            var glv = GlobalLoadVertex.GetInstance();
            data.AddMayWrite(glv, field);
        }

        public static PurityAnalysisData
            GetUnanalyzableCallSummary(string methodname, string containingType)
        {

            //var data = SummaryTemplates.CreatePureData();
            //var field = NamedField.New(containingType+"::"+methodname, String.Empty);
            //SummaryTemplates.WriteGlobalObject(data, field);
            //return data;
            var data = SummaryTemplates.CreatePureData();

            //if this is a constructor method
            if (PhxUtil.IsConstructor(methodname))
            {
                SummaryTemplates.MakeAllocator(data, containingType, methodname);
            }
            else
            {
                //make this an allocator method but the allocated object has unknown type
                //subjected to change
                SummaryTemplates.MakeAllocator(data, null, methodname);
            }
            return data;
        }

        //public static void ApplyWriterSummary(PurityAnalysisData data, Call call)
        //{
        //    //This could be accomplished using a wildcard fields
        //    //as of now not used anywhere.

        //    //HeapVertexSet paramNodes = new HeapVertexSet();            
        //    //var pointers = new HashSet<Pair<HeapVertexBase, Field>>();
        //    //data.OutHeapGraph.VisitBfs(
        //    //    call.GetAllParams().OfType<HeapVertexBase>(),
        //    //    null,
        //    //    (HeapEdgeBase e) => {
        //    //        if (e.Field != null)
        //    //        {
        //    //            pointers.Add(new Pair<HeapVertexBase, Field>(e.Source, e.Field));
        //    //        }
        //    //        return true;
        //    //    }
        //    //    );
        //    //data.MayWriteSet.UnionWith(pointers);
        //}

        //public static void MakeConstructorSummary(
        //    PurityAnalysisData outData, 
        //    string type,
        //    string methodName)
        //{
        //    //create a new object and add it to the callerData
            
        //        //update the type if possible
        //        //if (inst.FunctionSymbol.EnclosingAggregateType != null)
        //        //{
        //        //    var newObjType = PhxUtil.NormalizedAggregateType(inst.FunctionSymbol.EnclosingAggregateType);
        //        //    callerData.AddConcreteType(newObjectVertex, PhxUtil.GetTypeName(newObjType));
        //        //}            
        //}

        //public static void ApplyAllocatorSummary(
        //    PurityAnalysisData callerData, 
        //    Phx.IR.CallInstruction inst, 
        //    string type)
        //{
        //    //create a new object and add it to the callerData
        //    InternalHeapVertex newObjectVertex = NodeEquivalenceRelation.CreateInternalHeapVertex(
        //            inst.FunctionUnit.FunctionSymbol.QualifiedName,
        //            NodeEquivalenceRelation.GetInternalNodeId(inst),
        //            Context.EmptyContext);

        //    if (!callerData.OutHeapGraph.ContainsVertex(newObjectVertex))
        //    {
        //        callerData.OutHeapGraph.AddVertex(newObjectVertex);
        //        //update the type if possible
        //        if (inst.FunctionSymbol.EnclosingAggregateType != null)
        //        {
        //            var newObjType = PhxUtil.NormalizedAggregateType(inst.FunctionSymbol.EnclosingAggregateType);
        //            if(type != null)
        //                callerData.AddConcreteType(newObjectVertex, type);
        //        }
        //    }

        //    //make the destination operand point to new vertex
        //    var newObjList = new List<HeapVertexBase> { newObjectVertex };

        //    IHeapGraphOperandHandler handler;
        //    if (_operandHandlerProvider.TryGetHandler(inst.DestinationOperand, out handler))
        //    {
        //        handler.Write(inst.DestinationOperand, callerData, newObjList);
        //    }
        //}
    }
}
