using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using QuickGraph;

using SafetyAnalysis.Util;
namespace SafetyAnalysis.Framework.Graphs
{          
    public abstract class HeapVertexBase
    {
        private static int GUID = 7;        
        public int Id;       
                        
        protected HeapVertexBase()
        {
            this.Id = GUID++;
        }

        public override bool Equals(object obj)
        {
            return (this == obj);
        }
        
        public override int GetHashCode()
        {
            return this.Id;
        } 
        
        public abstract HeapVertexBase Copy();

        public abstract override string ToString();

        #region Serializable Members

        public abstract void GetObjectData(List<Pair<string,Object>> info);

        #endregion
    }

    public interface VertexWithSiteId
    {
        uint GetSiteId();
    }

    public abstract class VertexWithContext : HeapVertexBase
    {
        public Context context {get; private set;}

        protected VertexWithContext(Context ctx)
        {
            context = ctx;
        }
    }
}
