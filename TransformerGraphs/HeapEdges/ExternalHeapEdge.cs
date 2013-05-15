using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Framework.Graphs
{    
    public class ExternalHeapEdge : HeapEdgeBase
    {
        int hashCode = -1;        

        public ExternalHeapEdge(HeapVertexBase source, HeapVertexBase target, Field field)
            : base(source, target, field)
        {
            hashCode = -1;
        }

        public override HeapEdgeBase Copy()
        {
            return this;
        }

        public override bool Equals(object obj)
        {
            if (obj is ExternalHeapEdge)
            {
                ExternalHeapEdge edge = obj as ExternalHeapEdge;

                if (edge.Source.Equals(this.Source)
                    && edge.Target.Equals(this.Target)
                    && edge.Field.Equals(this.Field))
                    return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (hashCode == -1)
            {                
                hashCode = (Source.GetHashCode() << 7) ^ Target.GetHashCode() ^ Field.GetHashCode();
            }
            return hashCode;
        }
    }
}
