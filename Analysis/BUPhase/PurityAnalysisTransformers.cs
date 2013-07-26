using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Purity.Summaries;
using SafetyAnalysis.Purity.HandlerProvider;
using SafetyAnalysis.Util;
using SafetyAnalysis.TypeUtil;

namespace SafetyAnalysis.Purity
{
    public delegate void Transformer(Phx.IR.Instruction instr, PurityAnalysisData data);
 
    public class PurityAnalysisTransformers
    {
        public PurityAnalysisTransformers()
        {
            this._transformers = new Dictionary<Phx.Opcode, Transformer>();
            
            this._summaryHandlerProvider = 
                new PredicatedSummaryHandlerProvider<IPuritySummaryHandler>();
            
            this._operandHandlerProvider = 
                new PredicatedOperandHandlerProvider<IHeapGraphOperandHandler>();

            this.Initialize();
        }

        public virtual void Initialize()
        {
            // register transformers
            //binary operators
            _transformers.Add(Phx.Common.Opcode.Add, BinOP);
            _transformers.Add(Phx.Common.Opcode.Multiply, BinOP);
            _transformers.Add(Phx.Common.Opcode.Subtract, BinOP);
            _transformers.Add(Phx.Common.Opcode.Divide, BinOP);
            _transformers.Add(Phx.Common.Opcode.And, BinOP);
            _transformers.Add(Phx.Common.Opcode.Or, BinOP);            
            _transformers.Add(Phx.Common.Opcode.Le,BinOP);
            _transformers.Add(Phx.Common.Opcode.Ge,BinOP);
            _transformers.Add(Phx.Common.Opcode.Lt,BinOP);
            _transformers.Add(Phx.Common.Opcode.Gt,BinOP);
            _transformers.Add(Phx.Common.Opcode.Compare, BinOP);
            _transformers.Add(Phx.Common.Opcode.ShiftRight, BinOP);                        
            
            //unary operators
            _transformers.Add(Phx.Common.Opcode.Not, UnaryOP);
            _transformers.Add(Phx.Common.Opcode.Negate, UnaryOP);

            _transformers.Add(Phx.Common.Opcode.Assign, Assign);
            _transformers.Add(Phx.Common.Opcode.Call, Call);
            _transformers.Add(Phx.Common.Opcode.Calli, Calli);
            _transformers.Add(Phx.Common.Opcode.CallVirt, CallVirt);
            _transformers.Add(Phx.Common.Opcode.Box, CastClass);
            _transformers.Add(Phx.Common.Opcode.UnboxAny, CastClass);
            _transformers.Add(Phx.Common.Opcode.Unbox, CastClass); 
            _transformers.Add(Phx.Common.Opcode.CastClass, CastClass);
            _transformers.Add(Phx.Common.Opcode.IsInst, CastClass);            
            _transformers.Add(Phx.Common.Opcode.ConditionalBranch, ConditionalBranch);
            _transformers.Add(Phx.Common.Opcode.Convert, ConvertOp);
            _transformers.Add(Phx.Common.Opcode.EnterFunction, EnterFunction);
            _transformers.Add(Phx.Common.Opcode.Goto, Goto);
            _transformers.Add(Phx.Common.Opcode.InitObj, InitObj);
            _transformers.Add(Phx.Common.Opcode.LdFtn, LdFtn);
            _transformers.Add(Phx.Common.Opcode.LdVirtFtn, LdVirtFtn);
            _transformers.Add(Phx.Common.Opcode.LdSFld, LdSFld);
            _transformers.Add(Phx.Common.Opcode.StSFld, StSFld);
            _transformers.Add(Phx.Common.Opcode.LdStr, LdStr);
            _transformers.Add(Phx.Common.Opcode.NewObj, NewObj);
            _transformers.Add(Phx.Common.Opcode.NewArray, NewArray);
            _transformers.Add(Phx.Common.Opcode.Return, Return);            
            _transformers.Add(Phx.Common.Opcode.EnterCatch, EnterCatch);
            _transformers.Add(Phx.Common.Opcode.Throw, Throw);
            _transformers.Add(Phx.Targets.Architectures.Msil.Opcode.CONSTRAINEDPREFIX, ConstrainedPrefix);

            _operandHandlerProvider.RegisterHandler(new AddressOfOperandHandler());
            _operandHandlerProvider.RegisterHandler(new VariableOperandHandler());
            _operandHandlerProvider.RegisterHandler(new AbsoluteOperandHandler());            
            _operandHandlerProvider.RegisterHandler(new ComplexOperandHandler());
            _operandHandlerProvider.RegisterHandler(new SymbolicOperandHandler());
            _operandHandlerProvider.RegisterHandler(new PointerOperandHandler());
            _operandHandlerProvider.RegisterHandler(new ImmediateOperandHandler());
            
            _summaryHandlerProvider.RegisterHandler(new CallSummaryHandler());            
            _summaryHandlerProvider.RegisterHandler(new NewObjSummaryHandler());
        }

        public void Apply(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            if (!ShouldProcess(instruction))
                return;

            if (_transformers.ContainsKey(instruction.Opcode))
            {
                //Trace.TraceInformation("Processing instruction");
                //Trace.TraceInformation("Purity data\n{0}", data);
                //Trace.TraceInformation(instruction.ToString());

                Transformer transformer = _transformers[instruction.Opcode];
                transformer(instruction, data);

                //Trace.TraceInformation("Purity data\n{0}", data);
            }
            else
            {
                if (instruction.IsReal)
                {
                    Trace.TraceWarning("No transformer for instruction {0}", instruction);
                }
            }
        }

        private bool ShouldProcess(Phx.IR.Instruction instruction)
        {
            if (instruction.Opcode == Phx.Common.Opcode.Nop)
            {
                return false;
            }

            return true;
        }

        #region Transformer methods

        private void BinOP(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            var graph = data.OutHeapGraph;
            IEnumerable<HeapVertexBase> source1Vertices = null, source2Vertices = null;

            IHeapGraphOperandHandler sourceOperandHandler;
            if (_operandHandlerProvider.TryGetHandler(instruction.SourceOperand, out sourceOperandHandler))
            {
                source1Vertices = sourceOperandHandler.Read(instruction.SourceOperand1, data).ToList();
            }

            if (_operandHandlerProvider.TryGetHandler(instruction.SourceOperand2, out sourceOperandHandler))
            {
                source2Vertices = sourceOperandHandler.Read(instruction.SourceOperand2, data).ToList();
            }                
            IHeapGraphOperandHandler destinationOperandHandler;
            if (!_operandHandlerProvider.TryGetHandler(instruction.DestinationOperand, out destinationOperandHandler))
                return;

            if (source1Vertices != null && source1Vertices.Any())
                destinationOperandHandler.Write(instruction.DestinationOperand, data, source1Vertices);

            if (source2Vertices != null && source2Vertices.Any())
                destinationOperandHandler.Write(instruction.DestinationOperand, data, source2Vertices);
        }

        private void UnaryOP(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            Assign(instruction, data);
        }

        private void Assign(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            var graph = data.OutHeapGraph;

            IHeapGraphOperandHandler sourceOperandHandler;
            if (!_operandHandlerProvider.TryGetHandler(instruction.SourceOperand, out sourceOperandHandler))
                return;
            
            var sourceVertices = 
                sourceOperandHandler.Read(instruction.SourceOperand, data);

            IHeapGraphOperandHandler destinationOperandHandler;
            if (!_operandHandlerProvider.TryGetHandler(instruction.DestinationOperand, out destinationOperandHandler))
                return;

            destinationOperandHandler.Write(instruction.DestinationOperand, data, sourceVertices);
        }

        private void Call(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            IPuritySummaryHandler callHandler;
            if (_summaryHandlerProvider.TryGetHandler(instruction, out callHandler))
            {
                callHandler.ApplySummary(instruction.AsCallInstruction, data);
            }
        }

        private void Calli(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            Call(instruction, data);
        }

        private void CallVirt(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            Call(instruction, data);
        }

        private void CastClass(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            var graph = data.OutHeapGraph;

            //Contract.Assert(instruction.SourceOperand.IsImmediateOperand);
            //Contract.Assert(instruction.SourceOperand2 != null && instruction.SourceOperand2.IsExplicit);

            IHeapGraphOperandHandler sourceOperandHandler;
            if (!_operandHandlerProvider.TryGetHandler(instruction.SourceOperand2, out sourceOperandHandler))
                return;

            var sourceVertices =
                sourceOperandHandler.Read(instruction.SourceOperand2, data);

            IHeapGraphOperandHandler destinationOperandHandler;
            if (!_operandHandlerProvider.TryGetHandler(instruction.DestinationOperand, out destinationOperandHandler))
                return;

            destinationOperandHandler.Write(instruction.DestinationOperand, data, sourceVertices);
        }

        private void ConditionalBranch(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
        }
        
        private void ConvertOp(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            Assign(instruction, data);
        }

        private void EnterFunction(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {            
            var funcname = instruction.FunctionUnit.FunctionSymbol.QualifiedName;

            //Add the GlobalLoadVertex
            var glv = GlobalLoadVertex.GetInstance();
            if (!data.OutHeapGraph.ContainsVertex(glv))
                data.OutHeapGraph.AddVertex(glv);
                                    
            //add a return vertex and exception vertex
            var returnVertex = ReturnVertex.GetInstance();
            if (!data.OutHeapGraph.ContainsVertex(returnVertex))
                data.OutHeapGraph.AddVertex(returnVertex);

            var exceptionVertex = ExceptionVertex.GetInstance();
            if (!data.OutHeapGraph.ContainsVertex(exceptionVertex))
                data.OutHeapGraph.AddVertex(exceptionVertex);

            //Add the parameter nodes and edges from the formal parameter varaibles to the parameter nodes.
            uint parameterCount = 0;

            foreach (Phx.IR.Operand operand in instruction.ExplicitDestinationOperands)
            {                                
                //if (operand.Type.IsPrimitiveType)
                //    continue;

                parameterCount++;
                
                var parameterVertex = ParameterHeapVertex.New(parameterCount, operand.Symbol.NameString);                               

                if (!data.OutHeapGraph.ContainsVertex(parameterVertex))
                {
                    data.OutHeapGraph.AddVertex(parameterVertex);
                    AnalysisUtil.AddApproximateType(data, parameterVertex, operand.Type);                    
                }

                var variableVertex = NodeEquivalenceRelation.CreateVariableHeapVertex(
                    funcname, NodeEquivalenceRelation.GetVariableId(operand), Context.EmptyContext);
                
                if (!data.OutHeapGraph.ContainsVertex(variableVertex))
                    data.OutHeapGraph.AddVertex(variableVertex);

                var edge = new InternalHeapEdge(variableVertex, parameterVertex, null);
                if (!data.OutHeapGraph.ContainsHeapEdge(edge))
                    data.OutHeapGraph.AddEdge(edge);
            }
        }

        private void EnterCatch(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {            
            //the destination operand is the caught exception.
            Phx.IR.Operand destOperand = instruction.DestinationOperand1;
            //Contract.Assert(destOperand is Phx.IR.VariableOperand);

            IHeapGraphOperandHandler operandHandler;
            if (_operandHandlerProvider.TryGetHandler(destOperand, out operandHandler))
            {
                //assign the dest operand to the set of all vertices that is pointed to by the exceptionVertex.                
                var thrownVertices = (from edge in data.OutHeapGraph.OutEdges(
                                          ExceptionVertex.GetInstance())
                                      select edge.Target).ToList();

                operandHandler.Write(destOperand, data, thrownVertices);
            }
        }            

        private void Goto(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
        }

        private void InitObj(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            //make it call the constructor         
            var declaringtype = PhxUtil.GetTypeName(instruction.DestinationOperand.Type);
            var callingMethod = PhxUtil.GetQualifiedFunctionName(instruction.FunctionUnit.FunctionSymbol);
            var call = new StaticCall(AnalysisUtil.GetCallInstId(instruction),"ctor",declaringtype,"()void",new List<string> { callingMethod });
            var resolver = new CallBackResolver(instruction.FunctionUnit);
            resolver.ApplySummary(call, data, new HigherOrderHeapGraphBuilder(instruction.FunctionUnit));
        }

        /// <summary>
        /// This is like a new instruction that loads a function.
        /// In case of instance methods it is necessary to keep track of the receiver object
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="data"></param>
        private void LdFtn(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {       
            bool IsVirtual;
            if(instruction.Opcode.Equals(Phx.Common.Opcode.LdFtn))
                IsVirtual = false;
            else //Phx.Common.Opcode.LdVirtFtn
                IsVirtual = true;

            Phx.IR.Operand functionOperand = instruction.SourceOperand as Phx.IR.FunctionOperand;
            //Contract.Assert(functionOperand != null);
           // Console.WriteLine("Assigned funciton: " + functionOperand.Symbol.QualifiedName);

            var functionSym = PhxUtil.NormalizedFunctionSymbol(
                                    functionOperand.Symbol as Phx.Symbols.FunctionSymbol);
            var enclType = PhxUtil.NormalizedAggregateType(functionSym.EnclosingAggregateType);

            string typename = PhxUtil.GetTypeName(enclType);
            string methodname = PhxUtil.GetFunctionName(functionSym);
            string signature = PhxUtil.GetFunctionTypeSignature(functionSym.FunctionType);
            
            MethodHeapVertex methodVertex = MethodHeapVertex.New(
                typename, methodname, signature,
                functionSym.IsInstanceMethod, IsVirtual);            

            if (!data.OutHeapGraph.ContainsVertex(methodVertex))
                data.OutHeapGraph.AddVertex(methodVertex);            

            IHeapGraphOperandHandler destinationHandler;
            //Contract.Assert(instruction.DestinationOperand.IsVariableOperand);
            if (_operandHandlerProvider.TryGetHandler(instruction.DestinationOperand, out destinationHandler))
            {
                var objList = new List<HeapVertexBase> { methodVertex };
                destinationHandler.Write(instruction.DestinationOperand, data, objList);
            }
        }

        private void LdVirtFtn(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            //TODO: implement the LdVirtual function 
            LdFtn(instruction, data);
        }

        private void LdSFld(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            Assign(instruction, data);
        }

        /// <summary>
        /// Loading a string constant.
        /// TODO: use a single heap vertex to model string constants.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="data"></param>
        private void LdStr(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {            
            //approximate all constant strings using a single abstract object            
            var opname = instruction.SourceOperand1.AsImmediateOperand.Symbol.NameString;
            var strvalue = opname.Substring(opname.IndexOf('-') + 1);
            StringConstantVertex strconst = StringConstantVertex.New(strvalue);

            if (!data.OutHeapGraph.ContainsVertex(strconst))
            {
                data.OutHeapGraph.AddVertex(strconst);
                data.AddConcreteType(strconst, "[mscorlib]System.String");
            }

            IHeapGraphOperandHandler handler;
            if (_operandHandlerProvider.TryGetHandler(instruction.DestinationOperand, out handler))
            {
                handler.Write(instruction.DestinationOperand, data, new List<HeapVertexBase> { strconst });
            }
            else
                throw new SystemException("Cannot find operand handler");
        }

        private void NewObj(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            IPuritySummaryHandler newObjectHandler;
            if (_summaryHandlerProvider.TryGetHandler(instruction, out newObjectHandler))
            {
                newObjectHandler.ApplySummary(instruction.AsCallInstruction, data);
            }
            HandleDelegateCreation(instruction.AsCallInstruction, data);
        }

        //create an internal edge in the heap graph to capture delegate creation.
        private void HandleDelegateCreation(
            Phx.IR.CallInstruction instruction, 
            PurityAnalysisData data)
        {
            //is this a delegate object ?
            var moduleunit = instruction.FunctionUnit.ParentPEModuleUnit;
            var th = CombinedTypeHierarchy.GetInstance(moduleunit);

            Phx.Types.AggregateType type = PhxUtil.NormalizedAggregateType(
                instruction.FunctionSymbol.EnclosingAggregateType);
            var typename = PhxUtil.GetTypeName(type);
            var typeinfo = th.LookupTypeInfo(typename);
            if (!th.IsHierarhcyKnown(typeinfo))
                return;

            if (type != null && TypeMethodInfoUtil.IsDelegateType(th, typeinfo))
            {
                //make the delegate object point to methods it may dispatch to.     

                //first opeand SourceOperand2 is the receiver of the method 
                //if the method is an instance method otherwise it is null
                IHeapGraphOperandHandler sourceOperandHandler;
                if (_operandHandlerProvider.TryGetHandler(instruction.SourceOperand2,
                    out sourceOperandHandler))
                {
                    var recvrVertices = sourceOperandHandler.Read(instruction.SourceOperand2, data);

                    if (recvrVertices.Any())
                    {
                        (new DelegateOperandHandler()).Write(
                            instruction.DestinationOperand, DelegateRecvrField.GetInstance(), data, recvrVertices);
                    }
                }
                
                //read points-to vertices from the second argument (sourceOperand3) 
                //and map it to the destionation variable                
                if (_operandHandlerProvider.TryGetHandler(instruction.SourceOperand3, 
                    out sourceOperandHandler))
                {
                    var methodVertices = sourceOperandHandler.Read(instruction.SourceOperand3, data);

                    (new DelegateOperandHandler()).Write(
                        instruction.DestinationOperand, DelegateMethodField.GetInstance(), data, methodVertices);                                        
                }
            }
        }

        private void NewArray(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            InternalHeapVertex newArrayVertex =
                NodeEquivalenceRelation.CreateInternalHeapVertex(
                    instruction.FunctionUnit.FunctionSymbol.QualifiedName,
                    NodeEquivalenceRelation.GetInternalNodeId(instruction),
                    Context.EmptyContext);

            if (!data.OutHeapGraph.ContainsVertex(newArrayVertex))
            {
                data.OutHeapGraph.AddVertex(newArrayVertex);
                //update the concrete type of the object                                   
                data.AddConcreteType(newArrayVertex, PurityAnalysisData.ArrayType);
            }

            IHeapGraphOperandHandler destinationHandler;
            //Contract.Assert(instruction.DestinationOperand.IsVariableOperand);
            if (_operandHandlerProvider.TryGetHandler(instruction.DestinationOperand, out destinationHandler))
            {
                var newObjList = new List<HeapVertexBase> { newArrayVertex };
                destinationHandler.Write(instruction.DestinationOperand, data, newObjList);                
            }
        }

        private void Return(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            if (instruction.SourceOperand.IsLabelOperand)
                return;

            if (instruction.SourceOperand != null)
            {
                IHeapGraphOperandHandler handler;
                if (_operandHandlerProvider.TryGetHandler(instruction.SourceOperand, out handler))
                {
                    IEnumerable<HeapVertexBase> vertices = handler.Read(instruction.SourceOperand, data);
                    if (vertices.Any())
                    {
                        //make the return variable point to the vertices
                        var returnVertex = ReturnVertex.GetInstance();
                        foreach (var v in vertices)
                        {
                            InternalHeapEdge edge = new InternalHeapEdge(returnVertex, v, null);
                            if (!data.OutHeapGraph.ContainsHeapEdge(edge))
                                data.OutHeapGraph.AddEdge(edge);
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        private void Throw(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            if (instruction.SourceOperand.IsLabelOperand)
                return;

            if (instruction.SourceOperand != null)
            {
                IHeapGraphOperandHandler handler;
                if (_operandHandlerProvider.TryGetHandler(instruction.SourceOperand, out handler))
                {
                    var vertices = handler.Read(instruction.SourceOperand, data);
                    if (vertices.Any())
                    {
                        //make the exception variable point to the vertices
                        var exceptionVertex = ExceptionVertex.GetInstance();
                        foreach (var v in vertices)
                        {
                            InternalHeapEdge edge = new InternalHeapEdge(exceptionVertex, v, null);
                            if (!data.OutHeapGraph.ContainsHeapEdge(edge))
                                data.OutHeapGraph.AddEdge(edge);
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }                        
        }
        
        private void StSFld(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            if (instruction.DestinationOperand.NameString != null &&
                instruction.DestinationOperand.NameString.Contains("UNDEFINED"))
                System.Diagnostics.Debugger.Break();

            Assign(instruction, data);
        }

        private void ConstrainedPrefix(Phx.IR.Instruction instruction, PurityAnalysisData data)
        {
            //set a flag to say contrained prefix is set
            CallSummaryHandler.constrainedprefix = true;
        }

        #endregion 

        #region private fields

        private Dictionary<Phx.Opcode, Transformer> _transformers;
        private IPredicatedOperandHandlerProvider<IHeapGraphOperandHandler> _operandHandlerProvider;
        private IPredictedSummaryHandlerProvider<IPuritySummaryHandler> _summaryHandlerProvider;
        public static uint strconstid = 37;

        #endregion

    }
}
