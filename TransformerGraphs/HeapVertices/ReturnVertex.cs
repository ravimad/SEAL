using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;

namespace SafetyAnalysis.Framework.Graphs
{    
    public class ReturnVertex : HeapVertexBase
    {
        private static ReturnVertex _default = new ReturnVertex();

        public static ReturnVertex Create(List<Pair<string,Object>> info)            
        {
            return GetInstance();
        }

        public override HeapVertexBase Copy()
        {
            return _default;
        }

        //public override bool Equals(Object obj)
        //{
        //    return (obj is ReturnVertex);
        //}

        //public override int GetHashCode()
        //{
        //    return this.GetType().GetHashCode();
        //}

        public override string ToString()
        {
            return "_RETURN";
        }

        public static ReturnVertex GetInstance()
        {
            return _default;
        }

        public override void  GetObjectData(List<Pair<string,object>> info)
        {
 	
        }        
    }
}
