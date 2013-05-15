using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Util;
using SafetyAnalysis.TypeUtil;
using SafetyAnalysis.Purity;

namespace SafetyAnalysis.Purity.Summaries
{
    public class SyntheticCallStub : CallStubManager
    {
        private static SyntheticCallStub _instance = null;

        public static SyntheticCallStub GetInstance()
        {
            if (_instance == null)
                _instance = new SyntheticCallStub();
            return _instance;
        }

        private SyntheticCallStub() { }

        public bool HasSummary(Call call)
        {
            if (call is CallWithMethodName)
            {
                var mcall = call as CallWithMethodName;
                string qualtypename = mcall.GetDeclaringType();
                string methodname = mcall.GetMethodName();
                string signature = mcall.GetSignature();

                var qualname = AnalysisUtil.GetQualifiedName(call);
                if (qualname.Equals(
                    "[ScopeRuntime]ScopeRuntime.Row::get_Item/(System.String)[ScopeRuntime]ScopeRuntime.ColumnData"))
                    return true;
                else if (qualname.Equals(
                    "[ScopeRuntime]ScopeRuntime.Row::set_Item/(System.String,[ScopeRuntime]ScopeRuntime.ColumnData)void"))
                    return true;
                else if (qualtypename.Equals("[ScopeRuntime]ScopeRuntime.ColumnData")
                    && methodname.Equals("Set"))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Summaries could depend on the input data, that is, we allow 
        /// ceration of different summries for the same method based on the input data
        /// </summary>
        /// <param name="call"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public override PurityAnalysisData GetSummary(Call call, PurityAnalysisData data, CombinedTypeHierarchy th)
        {
            if (call is CallWithMethodName)
            {
                var mcall = call as CallWithMethodName;
                string qualtypename = mcall.GetDeclaringType();
                string methodname = mcall.GetMethodName();
                string signature = mcall.GetSignature();

                var qualname = AnalysisUtil.GetQualifiedName(call);
                if (qualname.Equals(
                    "[ScopeRuntime]ScopeRuntime.Row::get_Item/(System.String)[ScopeRuntime]ScopeRuntime.ColumnData"))
                {
                    return GetRowItem(call, data);
                }
                else if (qualname.Equals(
                    "[ScopeRuntime]ScopeRuntime.Row::set_Item/(System.String,[ScopeRuntime]ScopeRuntime.ColumnData)void"))
                {
                    return SetRowItem(call, data);
                }
                else if (qualtypename.Equals("[ScopeRuntime]ScopeRuntime.ColumnData")
                    && methodname.Equals("Set"))
                {
                    return SetColumnData(call);
                }
            }

            if (this.HasSummary(call))
                throw new SystemException("Cannot construct summary for the stubbed call: " + call);
            else
                throw new SystemException(call + " is  not stubbed");
        }

        private PurityAnalysisData SetRowItem(Call call, PurityAnalysisData data)
        {
            //create the fields from the input data
            var strvar = call.GetParam(1);
            var fields = from e in data.OutHeapGraph.OutEdges(strvar)
                         where e.Target is StringConstantVertex
                         select NamedField.New((e.Target as StringConstantVertex).Value, null);

            if (!fields.Any())
            {
                //heuristically use a wildcard field 
                fields = new List<NamedField> { NamedField.wildcard };
            }

            var newdata = SummaryTemplates.CreatePureData();
            var thisv = ParameterHeapVertex.New(1, "this");
            var param2 = ParameterHeapVertex.New(2, "key");
            var param3 = ParameterHeapVertex.New(3, "value");
            newdata.OutHeapGraph.AddVertex(thisv);
            newdata.OutHeapGraph.AddVertex(param2);
            newdata.OutHeapGraph.AddVertex(param3);

            foreach (var field in fields)
            {
                var edge = new InternalHeapEdge(thisv, param3, field);
                newdata.OutHeapGraph.AddEdge(edge);
            }
            return newdata;
        }

        private PurityAnalysisData GetRowItem(Call call, PurityAnalysisData data)
        {
            //create the fields from the input data
            var strvar = call.GetParam(1);
            var fields = from e in data.OutHeapGraph.OutEdges(strvar)
                         where e.Target is StringConstantVertex
                         select NamedField.New((e.Target as StringConstantVertex).Value, null);

            if (!fields.Any())
            {
                //heuristically use a wildcard field 
                fields = new List<NamedField> { NamedField.wildcard };
            }

            var newdata = SummaryTemplates.CreatePureData();
            var thisv = ParameterHeapVertex.New(1, "this");
            newdata.OutHeapGraph.AddVertex(thisv);

            var param2 = ParameterHeapVertex.New(2, "key");
            newdata.OutHeapGraph.AddVertex(param2);

            var loadv = NodeEquivalenceRelation.CreateLoadHeapVertex(AnalysisUtil.GetQualifiedName(call),
                call.contextid, Context.EmptyContext);
            newdata.OutHeapGraph.AddVertex(loadv);
            newdata.AddApproximateType(loadv, PurityAnalysisData.AnyType);

            foreach (var field in fields)
            {
                var edge = new ExternalHeapEdge(thisv, loadv, field);
                newdata.OutHeapGraph.AddEdge(edge);
            }
            newdata.OutHeapGraph.AddEdge(new InternalHeapEdge(ReturnVertex.GetInstance(), loadv, null));
            return newdata;
        }

        private PurityAnalysisData SetColumnData(Call call)
        {            
            var newdata = SummaryTemplates.CreatePureData();
            var thisv = ParameterHeapVertex.New(1, "this");
            var param2 = ParameterHeapVertex.New(2, "value");            
            newdata.OutHeapGraph.AddVertex(thisv);
            newdata.OutHeapGraph.AddVertex(param2);

            var field = NamedField.New("value", null);
            var edge = new InternalHeapEdge(thisv, param2, field);
            newdata.OutHeapGraph.AddEdge(edge);            
            return newdata;
        }        
    }
}
