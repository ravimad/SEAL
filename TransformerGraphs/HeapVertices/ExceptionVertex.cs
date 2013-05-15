using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;
namespace SafetyAnalysis.Framework.Graphs
{    
    public class ExceptionVertex : HeapVertexBase
    {
        private static ExceptionVertex _default = new ExceptionVertex();

        public static ExceptionVertex Create(List<Pair<string,Object>> info)            
        {
            return GetInstance();
        }

        public override HeapVertexBase Copy()
        {
            return this;
        }

        //public override bool Equals(Object obj)
        //{
        //    return (obj is ExceptionVertex);
        //}

        //public override  int GetHashCode()
        //{
        //    return this.GetType().GetHashCode();
        //}

        public override string ToString()
        {
            return "_EXCEPTION";
        }

        public static ExceptionVertex GetInstance()
        {
            return _default;
        }

        public override void  GetObjectData(List<Pair<string,Object>> info)
        {            
        }
    }

}
