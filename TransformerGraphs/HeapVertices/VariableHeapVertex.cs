using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using SafetyAnalysis.Util;

namespace SafetyAnalysis.Framework.Graphs
{
    using Variable = Triple<uint,string,Context>;
    
    public class VariableHeapVertex : VertexWithContext
    {
        public string functionName;        
        public uint index;                

        private static Dictionary<Variable, VariableHeapVertex> VariableTable
            = new Dictionary<Variable, VariableHeapVertex>();

        public static VariableHeapVertex New(string funcname, uint index, Context context)
        {
            var variable = new Variable(index, funcname, context);
            if (VariableTable.ContainsKey(variable))
                return VariableTable[variable];
            else
            {
                var vertex = new VariableHeapVertex(funcname, index, context);
                VariableTable.Add(variable, vertex);
                return vertex;
            }
        }

        private VariableHeapVertex(string funcname, uint index, Context ctx)
            : base(ctx)
        {
            this.index = index;
            this.functionName = funcname;
        }

        public static VariableHeapVertex Create(List<Pair<string, Object>> info)
        {
            var pair = info[0];
            if (!pair.Key.Equals("index"))
                throw new NotSupportedException("missing property index");
            var index = (uint)pair.Value;

            pair = info[1];
            if (!pair.Key.Equals("functionname"))
                throw new NotSupportedException("missing property function name");
            var name = (string)pair.Value;

            pair = info[2];
            if (!pair.Key.Equals("context"))
                throw new NotSupportedException("missing property context");
            var context = ((ContextWrapper)pair.Value).GetContext();

            return VariableHeapVertex.New(name, index, context);
        }
        
        public override HeapVertexBase Copy()
        {
            return this;
        }

        public override string ToString()
        {
            return functionName + ":" + index.ToString();
        }

        public override void GetObjectData(List<Pair<string,Object>> info)
        {           
            info.Add(new Pair<string,Object>("index",this.index));
            info.Add(new Pair<string, Object>("functionname", this.functionName));
            info.Add(new Pair<string, Object>("context", new ContextWrapper(this.context)));
        }    
    }
}
