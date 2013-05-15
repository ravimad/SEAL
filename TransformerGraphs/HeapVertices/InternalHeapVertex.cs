using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;
namespace SafetyAnalysis.Framework.Graphs
{
    using cidpair = Pair<uint, Context>;
    public class InternalHeapVertex : VertexWithContext,VertexWithSiteId
    {
        //function name could be included if needed
        public uint SiteId { get;  set; }

        private static Dictionary<cidpair, InternalHeapVertex> Table
            = new Dictionary<cidpair, InternalHeapVertex>();

        public static InternalHeapVertex New(uint siteId, Context context)
        {
            var pair = new cidpair(siteId, context);
            if (Table.ContainsKey(pair))
                return Table[pair];
            else
            {
                var vertex = new InternalHeapVertex(siteId, context);
                Table.Add(pair, vertex);
                return vertex;
            }
        }

        private InternalHeapVertex(uint siteId, Context context)            
            : base(context)
        {
            this.SiteId = siteId;                        
        }

        public static InternalHeapVertex Create(List<Pair<string,Object>> info)
        {
            var pair = info[0];
            if (!pair.Key.Equals("siteid"))
                throw new NotSupportedException("missing property siteid");
            var siteid = (uint)pair.Value;
            pair = info[1];
            if (!pair.Key.Equals("context"))
                throw new NotSupportedException("missing property context");
            var context = ((ContextWrapper)pair.Value).GetContext();
            return InternalHeapVertex.New(siteid, context);
        }
       
        public override string ToString()
        {
            return context.ToString() + ":" + SiteId.ToString();
        }

        public override HeapVertexBase Copy()
        {
            return this;
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
}
