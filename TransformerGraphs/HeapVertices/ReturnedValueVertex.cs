using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

using SafetyAnalysis.Util;
namespace SafetyAnalysis.Framework.Graphs
{
    using cidpair = Pair<uint, Context>;
    public class ReturnedValueVertex : VertexWithContext,VertexWithSiteId
    {        
        //siteId of the skipped call instruction
        public uint SiteId { get;  set; }

        private static Dictionary<cidpair, ReturnedValueVertex> Table
            = new Dictionary<cidpair, ReturnedValueVertex>();

        public static ReturnedValueVertex New(uint siteId, Context context)
        {
            var pair = new cidpair(siteId, context);
            if (Table.ContainsKey(pair))
                return Table[pair];
            else
            {
                var vertex = new  ReturnedValueVertex(siteId, context);
                Table.Add(pair, vertex);
                return vertex;
            }
        }

        private ReturnedValueVertex(uint siteId, Context context)          
            : base(context)
        {
            this.SiteId = siteId;                        
        }

        public static ReturnedValueVertex Create(List<Pair<string, Object>> info)
        {
            var pair = info[0];
            if (!pair.Key.Equals("siteid"))
                throw new NotSupportedException("missing property siteid");
            var siteid = (uint)pair.Value;
            pair = info[1];
            if (!pair.Key.Equals("context"))
                throw new NotSupportedException("missing property context");
            var context = ((ContextWrapper)pair.Value).GetContext();
            return ReturnedValueVertex.New(siteid, context);
        }

        public override HeapVertexBase Copy()
        {
            return this;
        }

        public override string ToString()
        {
            return SiteId.ToString();
        }

        public override void  GetObjectData(List<Pair<string,object>> info)
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
