//using System;
//using System.Collections.Generic;
//using System.Text;
//using SafetyAnalysis.Framework.Graphs;


//namespace SafetyAnalysis.Purity
//{
//    public class PurityAnalysisWalker : Phx.Dataflow.Walker
//    {
//        private string _description;
//        public string Description
//        {
//            get
//            {
//                return _description;
//            }
//            set
//            {
//                _description = value;
//            }
//        }

//        private PurityAnalysisTransformers _transformers;

//        public static void StaticInitialize()
//        {
//            if (_debugControl == null)
//            {
//                _debugControl =
//                    Phx.Controls.ComponentControl.New("Purity", "Purity Analysis", "PurityAnalysisWalker.cs");
//            }
//        }

//        public static PurityAnalysisWalker New(Phx.Lifetime lifetime, 
//            string description)
//        {
//            PurityAnalysisWalker walker = new PurityAnalysisWalker();

//            walker.Lifetime = lifetime;
//            walker.Description = description;
//            walker._transformers = new PurityAnalysisTransformers();
//            return walker;
//        }

//        public PurityAnalysisData Compute(Phx.FunctionUnit functionUnit, Phx.Dataflow.Direction direction)
//        {
//            //Contract.Assert(functionUnit.FlowGraph != null);
//            //Contract.Assert(direction != Phx.Dataflow.Direction.IllegalSentinel);

//            this.Initialize(direction, functionUnit);
//            this.Traverse(Phx.Dataflow.TraversalKind.Iterative, functionUnit);
            
//            PurityAnalysisData data = 
//                this.GetBlockData(functionUnit.LastExitInstruction.BasicBlock) as PurityAnalysisData;

//            return data;
//        }

//        #region Dataflow.Walker methods

//        public override void AllocateData(uint numberElements)
//        {
//            Phx.Dataflow.Data[] dataArray = new Phx.Dataflow.Data[numberElements];

//            for (uint i = 0; i < numberElements; i++)
//            {
//                dataArray[i] = PurityAnalysisData.New(this.Lifetime);
//            }

//            this.BlockDataArray = dataArray;
//        }

//        public override void EvaluateBlock(
//            Phx.Graphs.BasicBlock block, 
//            Phx.Dataflow.Data baseTemporaryData)
//        {
//            PurityAnalysisData temporaryData = baseTemporaryData as PurityAnalysisData;            
//            temporaryData.OutHeapGraph = temporaryData.OutHeapGraph.Copy();

//            foreach (Phx.IR.Instruction instruction in block.Instructions)
//            {                                
//                _transformers.Apply(instruction, temporaryData);                
//            }
//        }

//        static Phx.Controls.ComponentControl _debugControl;
//        public override Phx.Controls.ComponentControl DebugControl
//        {
//            get
//            {
//                return _debugControl;
//            }
//        }

//        public override void Dump()
//        {
//            Phx.FunctionUnit functionUnit =
//                Phx.Threading.Context.GetCurrent().FunctionUnit;

//            Phx.Output.WriteLine("{0}", this.Description);

//            Phx.Graphs.BasicBlock block = functionUnit.FlowGraph.BlockList;
//            while (block != null)
//            {
//                string label = string.Empty;
//                if (block.FirstInstruction.IsLabelInstruction)
//                    label = block.FirstInstruction.AsLabelInstruction.NameString;

//                PurityAnalysisData data = this.GetBlockData(block) as PurityAnalysisData;

//                Phx.Output.WriteLine("\tBasic block {0} ({1})", block.Id, label);
//                data.BlockId = block.Id;
//                data.Dump();

//                block = block.Next;
//            }
//        }

//        public override void Delete()
//        {
//            base.Delete();
//        }

//        #endregion
//    }
//}
