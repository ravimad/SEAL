using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

using SafetyAnalysis.Util;
namespace SafetyAnalysis.Framework.Graphs
{    
    [Serializable]    
    public class HeapNodeWrapper
    {
        string typename;
        List<Pair<string, Object>> info = new List<Pair<string,object>>();

        public HeapNodeWrapper(HeapVertexBase v)
        {
            typename = v.GetType().ToString();
            v.GetObjectData(info);
        }
        
        public HeapVertexBase GetNode()
        {
            var objCreator = this.GetType().Assembly.GetType(typename).GetMethod("Create");
            var node = objCreator.Invoke(null, new Object[] { info });
            return node as HeapVertexBase;
        }        
    }

    [Serializable]    
    public class FieldWrapper
    {
        string typename = null;
        List<Pair<string, Object>> info= new List<Pair<string,object>>();

        public FieldWrapper(Field f)
        {
            if (f != null)
            {
                typename = f.GetType().ToString();
                f.GetObjectData(info);
            }
        }

        public Field GetField()
        {
            if (typename != null)
            {
                var objCreator = this.GetType().Assembly.GetType(typename).GetMethod("Create");
                var node = objCreator.Invoke(null, new Object[] { info });
                return node as Field;
            }
            return null;
        }
    }

    [Serializable]    
    public class HeapEdgeWrapper
    {
        HeapNodeWrapper src;
        HeapNodeWrapper tgt;
        FieldWrapper field;
        bool internalEdge;

        public HeapEdgeWrapper(HeapEdgeBase edge)
        {
            src = new HeapNodeWrapper(edge.Source);
            tgt = new HeapNodeWrapper(edge.Target);              
            field = new FieldWrapper(edge.Field);
            internalEdge = edge is InternalHeapEdge;
        }

        public HeapEdgeBase GetEdge()
        {
            HeapEdgeBase edge;
            if (internalEdge)
                edge = new InternalHeapEdge(src.GetNode(), tgt.GetNode(), field.GetField());
            else
                edge = new ExternalHeapEdge(src.GetNode(), tgt.GetNode(), field.GetField());
            return edge;
        }
    }

    [Serializable]
    public class ContextWrapper
    {        
        List<object> ctstr;

        public ContextWrapper(Context c)
        {
            ctstr = c.GetContextString();
        }

        public Context GetContext()
        {
            return Context.New(ctstr);
        }
    }
}
