using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.Serialization;
using System.IO;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.Summaries;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity
{    
    /// <summary>
    /// Implements a PurityAnalysisData with shared state
    /// </summary>
    [Serializable]
    public class PartitionPurityData : PurityAnalysisData
    {        
        //transformer graph part            
        private PartitionGraph pg; 
        public override HeapGraphBase OutHeapGraph { 
            get { return pg; }
            protected set { pg = value as PartitionGraph; }   
        }

        //associating a version number with skipped calls
        private uint skcallVersion = 0;
        
        //indicates changes done to the shared state after the last copy
        public uint SharedVersion
        {
            get { return skcallVersion + this.pg.SharedVersion; }
        }
                        
        public PartitionPurityData(PartitionGraph pg)
            : base(pg)
        {                        
        }

        public PartitionPurityData(SerializationInfo info, StreamingContext context)
            : base(info,context)
        {                                    
        }                            
        
        public PartitionPurityData CopyNonShared()
        {          
            var data = new PartitionPurityData(this.pg.CopyNonShared());

            //the rest of the state is always shared
            data.skippedCalls = this.skippedCalls;            
            data.skippedCallTargets = this.skippedCallTargets;
            data.types = this.types;
            data.MayWriteSet = this.MayWriteSet;
            //data.ReadSet = this.ReadSet;
            //data.MustWriteSet = this.MustWriteSet;          

            //copy auxiliary state
            data.linqMethods = this.linqMethods;
            data.strongUpdateVertices = this.strongUpdateVertices;
            data.unanalyzableCalls = this.unanalyzableCalls;

            //copy the version number of the skipped call
            data.skcallVersion = this.skcallVersion;
            return data;
        }

        public void UnionNonShared(PartitionPurityData data)
        {
            //union only the graph part
            this.pg.UnionNonShared(data.pg);
        }

        public bool NonSharedEquivalent(PartitionPurityData purityData)
        {            
            return this.pg.EqualsNonShared(purityData.pg);
        }
                
        public override void RemoveSkippedCall(Call c)
        {
            base.RemoveSkippedCall(c);
            //increment the version number
            skcallVersion++;
        }               
                
        public override void AddSkippedCall(Call c)
        {
            if (!this.skippedCalls.Contains(c))
            {
                this.skippedCalls.Add(c);
                //increment the version number
                skcallVersion++;
            }
        }

        public PurityAnalysisData ConvertToPlainData()
        {
            var data = new PurityAnalysisData(new HeapGraph());
            data.Union(this);
            return data;
        }
    }
}
