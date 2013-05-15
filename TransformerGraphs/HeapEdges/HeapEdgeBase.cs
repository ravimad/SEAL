using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.Contracts;

namespace SafetyAnalysis.Framework.Graphs
{    
    public abstract class HeapEdgeBase : QuickGraph.Edge<HeapVertexBase>
    {       
        public Field Field { get; set; }

        public HeapEdgeBase(HeapVertexBase source, HeapVertexBase target, Field field)
            : base(source, target)
        {
            if (field == null)
                field = NullField.Instance;
            this.Field = field;
        }
            
        public override string ToString()
        {
            string str = "";
            if (Source != null)
                str += this.Source.ToString();
            str += "--" + this.Field.ToString();
            if(Target!= null)
                str += "-->" + this.Target.ToString();
            return str;
        } 

        public abstract HeapEdgeBase Copy();
    }
}
