using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;
namespace SafetyAnalysis.Framework.Graphs
{    
    public abstract class ConstantVertexBase : HeapVertexBase
    {        
        public override HeapVertexBase Copy()
        {
            return this;
        }     
    }
}
