using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

using SafetyAnalysis.Util;
namespace SafetyAnalysis.Framework.Graphs
{    
    using cidpair = Pair<uint,Context>;
    public class LoadHeapVertex : VertexWithContext,VertexWithSiteId
    {        
        public uint SiteId { get;  set; }

        private static Dictionary<cidpair, LoadHeapVertex> Table
            = new Dictionary<cidpair, LoadHeapVertex>();

        public static LoadHeapVertex New(uint siteId, Context context)
        {
            var pair = new cidpair(siteId, context);
            if (Table.ContainsKey(pair))
                return Table[pair];
            else
            {
                var vertex = new LoadHeapVertex(siteId, context);
                Table.Add(pair, vertex);
                return vertex;
            }
        }

        protected LoadHeapVertex(uint siteId, Context context) : base(context)          
        {
            this.SiteId = siteId;                        
        }

        public static LoadHeapVertex Create(List<Pair<string, Object>> info)
        {
            var pair = info[0];
            if (!pair.Key.Equals("siteid"))
                throw new NotSupportedException("missing property siteid");
            var siteid = (uint)pair.Value;
            pair = info[1];
            if(!pair.Key.Equals("context"))
                throw new NotSupportedException("missing property context");
            var context = ((ContextWrapper)pair.Value).GetContext();
            return LoadHeapVertex.New(siteid,context);
        }       

        public override HeapVertexBase Copy()
        {
            return this;
        }

        public override string ToString()
        {
            return context.ToString()+":"+SiteId.ToString();
        }

        public override void GetObjectData(List<Pair<string, object>> info)
        {
            info.Add(new Pair<string, Object>("siteid", this.SiteId));
            info.Add(new Pair<string, Object>("context", new ContextWrapper(this.context)));
        }

        public uint GetSiteId()
        {
            return this.SiteId;
        }
    }

    //Singleton classes    
    public class GlobalLoadVertex : LoadHeapVertex
    {
        private static GlobalLoadVertex _default = new GlobalLoadVertex();

        public GlobalLoadVertex() : base(0,Context.EmptyContext)
        {            
        }

        public static new GlobalLoadVertex Create(List<Pair<string, Object>> info)
        {
            return GetInstance();
        }

        public override string ToString()
        {
            return "_GLOBAL";
        }

        public static GlobalLoadVertex GetInstance()
        {            
            return _default;
        }

        public override void GetObjectData(List<Pair<string, object>> info)
        {            
        }        
    }
}
