using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;
using SafetyAnalysis.Purity;
using QuickGraph;
using QuickGraph.Algorithms.Search;
using Phx.Types;

namespace SafetyAnalysis.TypeUtil
{
    using Query = Pair<TypeInfo, TypeInfo>;
    public class CombinedTypeHierarchy
    {        
        private BidirectionalGraph<TypeInfo, Edge<TypeInfo>> th = null;
        private Dictionary<string, TypeInfo> TypeTable = new Dictionary<string, TypeInfo>();
        private HashSet<TypeInfo> rootDelegateInfos = new HashSet<TypeInfo>();
        private HashSet<TypeInfo> delegateTypes = null;
     
        //one type hierarchy per assembly
        private static Dictionary<string, CombinedTypeHierarchy> _instances = new Dictionary<string, CombinedTypeHierarchy>();

        //records the root delegate types
        public static HashSet<string> delegateRoots = new HashSet<string>(new string[] { "System.Func", "System.Action", "System.Delegate", 
                                                                                          "System.MulticastDelegate", "System.EventHandler" });        

        public static CombinedTypeHierarchy GetInstance(Phx.PEModuleUnit moduleUnit)
        {
            var assemblyname = moduleUnit.Manifest.Name.NameString;
            CombinedTypeHierarchy instance;
            if (!_instances.TryGetValue(assemblyname, out instance))
            {
                instance = new CombinedTypeHierarchy();
                instance.ConstructTypeHierarchy(moduleUnit);
                _instances.Add(assemblyname, instance);             
            }
            return instance;
        }

        public static void ReleaseModuleUnit(Phx.PEModuleUnit moduleUnit)
        {
            var assemblyname = moduleUnit.Manifest.Name.NameString;
            CombinedTypeHierarchy instance;
            if (_instances.TryGetValue(assemblyname, out instance))
            {
                instance.CleanUp();
            }
        }

        private static void RecordIfDelegateRoot(CombinedTypeHierarchy th, TypeInfo info)
        {
            var uname = PhxUtil.RemoveAssemblyName(info.GetTypeName());
            //remove the generic parameters
            var i = uname.IndexOf('`');
            if (i >= 0)
                uname = uname.Substring(0, i);
            if (delegateRoots.Contains(uname))
            {
                th.rootDelegateInfos.Add(info);
            }
        }

        private void ConstructTypeHierarchy(Phx.PEModuleUnit moduleUnit)
        {
            th = new BidirectionalGraph<TypeInfo, Edge<TypeInfo>>();

            //populate internal types            
            PopulateInternalTypes(moduleUnit);

            //populate external types
            if (!PurityAnalysisPhase.DisableExternalCallResolution)
                PopulateExternalTypes();
        }        

        private TypeInfo CreateInternalType(AggregateType aggty)
        {
            var name = PhxUtil.GetTypeName(aggty);
            if (TypeTable.ContainsKey(name))
                return TypeTable[name];
            else
            {
                var typeinfo = InternalTypeInfo.New(aggty, name, this);
                TypeTable.Add(name, typeinfo);

                //check if this is a root delegate type 
                RecordIfDelegateRoot(this,typeinfo);

                return typeinfo;
            }
        }        

        private TypeInfo CreateExternalType(string name)
        {
            if (TypeTable.ContainsKey(name))
                return TypeTable[name];
            else
            {
                var typeinfo = ExternalTypeInfo.New(name, this);
                TypeTable.Add(name, typeinfo);

                //check if this is a root delegate type 
                RecordIfDelegateRoot(this, typeinfo);

                return typeinfo;
            }
        }

        private void PopulateInternalTypes(Phx.PEModuleUnit moduleUnit)
        {
            foreach (var type in moduleUnit.AllTypes)
            {
                //note: type could be internal or external type
                if (type.IsAggregateType && type.TypeSymbol != null)
                    //&& PhxUtil.GetAssemblySymbol(type.TypeSymbol) != null)
                {                    
                    var aggtype = type.AsAggregateType;
                    TypeInfo typeinfo;

                    var asmsym = PhxUtil.GetAssemblySymbol(type.TypeSymbol);
                    if(asmsym == null)
                        typeinfo = CreateInternalType(aggtype);
                    else if (PhxUtil.DoesBelongToCurrentAssembly(type.TypeSymbol, moduleUnit))
                        typeinfo = CreateInternalType(aggtype);
                    else
                        typeinfo = CreateExternalType(PhxUtil.GetTypeName(aggtype));                   

                    if (!th.ContainsVertex(typeinfo))
                    {
                        th.AddVertex(typeinfo);                        
                    }

                    //get normalized aggtype
                    var normaggtype = PhxUtil.NormalizedAggregateType(aggtype);

                    //add edges from the super types
                    if (normaggtype.BaseTypeLinkList != null)
                    {
                        var list = normaggtype.BaseTypeLinkList;
                        while (list != null)
                        {
                            var suptype = PhxUtil.NormalizedAggregateType(list.BaseAggregateType);
                            TypeInfo suptypeinfo;

                            if (PhxUtil.DoesBelongToCurrentAssembly(suptype.TypeSymbol, moduleUnit))
                                suptypeinfo = CreateInternalType(suptype);
                            else 
                                suptypeinfo = CreateExternalType(PhxUtil.GetTypeName(suptype));                            

                            //add the super type to the hierarchy
                            if (!th.ContainsVertex(suptypeinfo))
                            {
                                th.AddVertex(suptypeinfo);                                
                            }

                            //add an edge from the suptype to the base type
                            if (!th.ContainsEdge(suptypeinfo, typeinfo))
                            {
                                var edge = new Edge<TypeInfo>(suptypeinfo, typeinfo);
                                th.AddEdge(edge);
                            }
                            list = list.Next;
                        }
                    }
                }
            }
        }

        private void PopulateExternalTypes()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Start();
            
            //use the data base context here.
            PurityDBDataContext dbcontext = PurityAnalysisPhase.DataContext;
            foreach (var record in dbcontext.TypeHierarchies)
            {
                var typeinfo = CreateExternalType(record.Typename);                               

                if (!th.ContainsVertex(typeinfo))
                {
                    th.AddVertex(typeinfo);                    
                }

                if (String.IsNullOrEmpty(record.SuperTypename))
                    continue;

                var suptypeinfo = CreateExternalType(record.SuperTypename);
                
                if (!th.ContainsVertex(suptypeinfo))
                {
                    th.AddVertex(suptypeinfo);                    
                }
                var edge = new Edge<TypeInfo>(suptypeinfo, typeinfo);
                th.AddEdge(edge);
            }
            dbcontext.Dispose();

            sw.Stop();
            if (PurityAnalysisPhase.EnableStats)
                MethodLevelAnalysis.dbaccessTime += sw.ElapsedMilliseconds;
        }

        public IEnumerable<TypeInfo> GetSubTypesFromTypeHierarchy(TypeInfo rootType)
        {            
            //do a traversal of the type hierarchy from the given node.            
            if (!th.ContainsVertex(rootType))
                throw new SystemException("Root type: " + rootType + " does not exist in the type hierarchy");

            BreadthFirstSearchAlgorithm<TypeInfo, Edge<TypeInfo>>
                bfs = new BreadthFirstSearchAlgorithm<TypeInfo, Edge<TypeInfo>>(th);

            bfs.SetRootVertex(rootType);
            var reachableVertices = new HashSet<TypeInfo>();
            bfs.DiscoverVertex += (TypeInfo vertex) => { reachableVertices.Add(vertex); };
            bfs.Compute();
            return reachableVertices;
        }

        public IEnumerable<TypeInfo> GetSuperTypesFromTypeHierarhcy(TypeInfo rootType)
        {
            ReversedBidirectionalGraph<TypeInfo, Edge<TypeInfo>> reverseGraph
                = new ReversedBidirectionalGraph<TypeInfo, Edge<TypeInfo>>(th);

            BreadthFirstSearchAlgorithm<TypeInfo, SReversedEdge<TypeInfo, Edge<TypeInfo>>> bfs
                = new BreadthFirstSearchAlgorithm<TypeInfo, SReversedEdge<TypeInfo, Edge<TypeInfo>>>(reverseGraph);

            bfs.SetRootVertex(rootType);
            HashSet<TypeInfo> reachableVertices = new HashSet<TypeInfo>();
            bfs.DiscoverVertex += (TypeInfo vertex) => { reachableVertices.Add(vertex); };
            bfs.Compute();
            return reachableVertices;
        }

        public IEnumerable<TypeInfo> GetParents(TypeInfo type)
        {
            var parents = from edge in th.InEdges(type)
                          select edge.Source;
            return parents;
        }

        /// <summary>
        /// Checks if t1 is a super type of t2
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public bool IsSuperType(TypeInfo t1, TypeInfo t2)
        {
            ReversedBidirectionalGraph<TypeInfo, Edge<TypeInfo>> reverseGraph
                = new ReversedBidirectionalGraph<TypeInfo, Edge<TypeInfo>>(th);

            BreadthFirstSearchAlgorithm<TypeInfo, SReversedEdge<TypeInfo, Edge<TypeInfo>>> bfs
                = new BreadthFirstSearchAlgorithm<TypeInfo, SReversedEdge<TypeInfo, Edge<TypeInfo>>>(reverseGraph);

            bfs.SetRootVertex(t2);
            bool found = false;
            bfs.DiscoverVertex += (TypeInfo vertex) => {
                if (t1.Equals(vertex))
                {
                    found = true;
                    bfs.Abort();
                }                
            };
            bfs.Compute();
            return found;
        }

        public bool AreRelated(TypeInfo t1, TypeInfo t2)
        {
            return (this.IsSuperType(t1, t2) || this.IsSuperType(t2, t1));
        }

        public void ResolveSuptypeQueries(Dictionary<Query, bool> suptypeQueries)
        {
            //do a dfs
            var reverseGraph = new ReversedBidirectionalGraph<TypeInfo, Edge<TypeInfo>>(th);            

            //add all roots
            var queue = new Queue<TypeInfo>();
            var processedValues = new HashSet<TypeInfo>();
            foreach (var query in suptypeQueries.Keys)
            {
                var root = query.Value;
                if (processedValues.Contains(root))
                    continue;
                processedValues.Add(root);
                queue.Enqueue(root);
            }            

            while (queue.Any())
            {
                var root = queue.Dequeue();

                var bfs = new BreadthFirstSearchAlgorithm<TypeInfo, SReversedEdge<TypeInfo, Edge<TypeInfo>>>(reverseGraph);
                bfs.SetRootVertex(root);
                bfs.DiscoverVertex += (TypeInfo vertex) =>
                {
                    var q = new Query(vertex, root);
                    if (suptypeQueries.ContainsKey(q))
                        suptypeQueries[q] = true;
                };
                bfs.Compute();
            }            
        }

        public List<MethodInfo> GetInheritedMethods(
            TypeInfo typeinfo,
            string methodName,
            string methodSignature)
        {
            //TODO make function signatures exact            
            List<MethodInfo> result = new List<MethodInfo>();
            
            //check if the type directly defines (or overrides) the method                        
            //if the method is not defined in this class search its base classes as it might have been inherited 
            var queue = new Queue<TypeInfo>();            
            queue.Enqueue(typeinfo);
            while (queue.Any())
            {
                var info  = queue.Dequeue();
                foreach (var methodinfo in info.GetMethodInfos(methodName, methodSignature))
                {
                    result.Add(methodinfo);                    
                }
                if (result.Any())
                {
                    //In case multiple base classes define the same method,
                    //any one of them would be arbitrarily picked
                    break;
                }

                //enqueue all the parents
                foreach (var parent in this.GetParents(info))
                {
                    if (!parent.IsInterface())
                        queue.Enqueue(parent);
                }
            }            
            return result;
        }

        public TypeInfo LookupTypeInfo(string typename)
        {            
            TypeInfo typeinfo = null;
            if (TypeTable.TryGetValue(typename, out typeinfo))
                return typeinfo;
            else
            {
                //this could be a type for which stubs are specified
                typeinfo = ExternalTypeInfo.New(typename, this);
                TypeTable.Add(typename, typeinfo);
                this.th.AddVertex(typeinfo);
                return typeinfo;
            }
        }

        public bool IsHierarhcyKnown(TypeInfo tinfo)
        {
            //for now we consider only types that hasInfo as completely available.
            //this may  be unsound as the complete call-chain may not be known
            return tinfo.HasInfo();
        }

        public void SerializeInternalTypes(Phx.PEModuleUnit moduleUnit,
            PurityDBDataContext dbContext)
        {
            string dllname = moduleUnit.Manifest.Name.NameString;            
            HashSet<Pair<string, string>> typepairs = new HashSet<Pair<string, string>>();

            foreach (var intTypeinfo in th.Vertices.OfType<InternalTypeInfo>())
            {                
                var aggtype = intTypeinfo.Aggtype;

                //serialize all types irrespective of whether they are analyzable or not
                //however dont serialize instantiaons and types that has no associated assembly symbol
                if (aggtype.UninstantiatedAggregateType != null
                    || PhxUtil.GetAssemblySymbol(aggtype.TypeSymbol) == null)
                    continue;

                Console.WriteLine("serializing aggregate type: " + aggtype.TypeSymbol.QualifiedName);
                if (intTypeinfo.Typename.Length >= 200)
                {
                    throw new NotSupportedException("Cannot serialize type: "+ intTypeinfo.Typename +
                        " Type length: " + intTypeinfo.Typename.Length);
                }

                //serialize type and super type
                var parents = this.GetParents(intTypeinfo);                

                if (!parents.Any())
                {
                    TypeHierarchy typeHierarchy = new TypeHierarchy();
                    typeHierarchy.Typename = intTypeinfo.Typename;
                    typeHierarchy.dllname = dllname;
                    typeHierarchy.SuperTypename = String.Empty;
                    dbContext.TypeHierarchies.InsertOnSubmit(typeHierarchy);                    
                }
                else
                {
                    foreach (var parent in parents)
                    {                                                
                        var parentTypename = parent.GetTypeName();

                        if (parentTypename.Length >= 200)
                        {
                            throw new NotSupportedException("Cannot serialize parent type: "+
                                parentTypename + " Type length: " + parentTypename.Length);
                        }

                        var pair = new Pair<string, string>(intTypeinfo.Typename, parentTypename);
                        if (typepairs.Contains(pair))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Duplicate: " + pair.Key + "," + pair.Value);
                            Console.ResetColor();
                            continue;
                        }

                        TypeHierarchy typeHierarchy = new TypeHierarchy();
                        typeHierarchy.Typename = intTypeinfo.Typename;
                        typeHierarchy.dllname = dllname;
                        typeHierarchy.SuperTypename = parentTypename;
                        dbContext.TypeHierarchies.InsertOnSubmit(typeHierarchy);

                        typepairs.Add(pair);
                    }
                }                

                //serialize type info
                intTypeinfo.Serialize(moduleUnit,dbContext);
            }                                    
        }       

        //additional utility methods        
        public bool IsDelegateType(TypeInfo typeinfo)
        {
            //populate delegate types if it is null
            if (delegateTypes == null)
            {
                delegateTypes = new HashSet<TypeInfo>();
                foreach(var root in rootDelegateInfos)
                {
                    delegateTypes.UnionWith(this.GetSubTypesFromTypeHierarchy(root));
                }
            }
            return this.delegateTypes.Contains(typeinfo);
        }

        public void CleanUp()
        {
            this.th = null;            
            this.TypeTable = null;
            this.delegateTypes = null;
            this.rootDelegateInfos = null;
        }
    }    
}
