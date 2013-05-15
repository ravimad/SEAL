using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;
namespace SafetyAnalysis.Framework.Graphs
{    
    using TabKey = Pair<uint,string>;
    public class ParameterHeapVertex : HeapVertexBase
    {        
        public uint Index { get;  set; }
        //name is not an identifier it is just meta data
        public string name;

        /// <summary>
        /// Do not do pool allocation here because of other dependences
        /// particularly in PurityDataUtil::RenameParameters
        /// </summary>
        /// <param name="index"></param>
        /// <param name="paramname"></param>
        /// <returns></returns>
        public static ParameterHeapVertex New(uint index, string paramname)
        {           
            return new ParameterHeapVertex(index, paramname);
        }

        private ParameterHeapVertex(uint index, string paramname)            
        {
            this.Index = index;
            this.name = paramname;
        }

        public static ParameterHeapVertex Create(List<Pair<string, Object>> info)
        {
            var pair = info[0];
            if (!pair.Key.Equals("index"))
                throw new NotSupportedException("missing property index");
            var index = (uint)pair.Value;
            
            pair = info[1];
            if (!pair.Key.Equals("paramname"))
                throw new NotSupportedException("missing property name");
            var name = (string)pair.Value;

            return ParameterHeapVertex.New(index, name);
        }

        /// <summary>
        /// Should not include the name in comparison as this should match different vertices 
        /// used by overriden methods
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var vertex = obj as ParameterHeapVertex;

            if (vertex == null)
                return false;

            if (vertex.Index != this.Index)
                return false;            

            return true;
        }

        public override int GetHashCode()
        {
            return (this.GetType().GetHashCode() << 10) ^ (int)this.Index;
        }

        public override HeapVertexBase Copy()
        {
            return this;
        }

        public override string ToString()
        {
            return Index.ToString() + ":" + (name == null ? "" : name);
        }

        public override void GetObjectData(List<Pair<string,Object>> info)
        {           
            info.Add(new Pair<string,Object>("index",this.Index));
            info.Add(new Pair<string, Object>("paramname", this.name));
        }
    }
}
