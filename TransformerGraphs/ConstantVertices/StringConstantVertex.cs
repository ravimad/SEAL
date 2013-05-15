using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;
namespace SafetyAnalysis.Framework.Graphs
{    
    public class StringConstantVertex : ConstantVertexBase
    {                
        public string Value { get; private set; }

        private static Dictionary<string, StringConstantVertex> Table
            = new Dictionary<string, StringConstantVertex>();

        public static StringConstantVertex New(string value)
        {
            if (Table.ContainsKey(value))
                return Table[value];
            else
            {
                var vertex = new StringConstantVertex(value);
                Table.Add(value, vertex);
                return vertex;
            }
        }

        private StringConstantVertex(string value)                 
        {
            this.Value = value;                  
        }

        public static StringConstantVertex Create(List<Pair<string,Object>> info)
        {
            var pair = info[0];
            if (!pair.Key.Equals("value"))
                throw new NotSupportedException("missing property string value");
            var val = (string)pair.Value;            
            return StringConstantVertex.New(val);
        }
       
        public override string ToString()
        {
            return this.Value;
        }

        public override void GetObjectData(List<Pair<string, object>> info)
        {
            info.Add(new Pair<string, Object>("value", this.Value));            
        }         

        /// <summary>
        /// A temporary hack for determining if a field represents a column
        /// </summary>
        /// <returns></returns>
        public static bool ExistsStrConst(string val)
        {
            return Table.ContainsKey(val);
        }
    }
}
