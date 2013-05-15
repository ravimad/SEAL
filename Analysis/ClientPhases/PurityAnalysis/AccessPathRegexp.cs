using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using SafetyAnalysis.Framework.Graphs;
using SafetyAnalysis.Util;

namespace SafetyAnalysis.Purity
{
    /// <summary>
    /// can only support a linear sequence of fields as of now.
    /// cannot support kleen's closure etc.
    /// </summary>
    [Serializable]
    public class AccessPathRegexp : ISerializable
    {
        public List<Field> fields = new List<Field>();
        public HeapVertexBase root;

        //used only for serialization
        private AccessPathRegexp()
        {
        }        

        public AccessPathRegexp(HeapVertexBase v, IEnumerable<Field> col)
        {
            root = v;
            if (col != null)
                AppendFields(col);
        }

        public AccessPathRegexp(SerializationInfo info, StreamingContext context)
        {
            fields = new List<Field>();
            var fieldWraps = (List<FieldWrapper>)info.GetValue("fieldlist",typeof(List<FieldWrapper>));
            foreach(var fieldWrap in fieldWraps)
                fields.Add(fieldWrap.GetField());
            var rootWrap = (HeapNodeWrapper)info.GetValue("root", typeof(HeapNodeWrapper));
            root = rootWrap.GetNode();
        }

        public void AppendField(Field f)
        {
            fields.Add(f);
        }

        public void AppendFields(IEnumerable<Field> col)
        {
            fields.AddRange(col);
        }

        public override bool Equals(Object obj)
        {
            if (obj is AccessPathRegexp)
            {
                AccessPathRegexp ap = obj as AccessPathRegexp;
                if (this.root.Equals(ap.root)
                    && this.fields.SequenceEqual(ap.fields, new WitnessComparer()))
                    return true;                
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (root.GetHashCode() << 7) ^ fields.Count;
        }
        
        public AccessPathRegexp Clone()
        {
            return new AccessPathRegexp(root, fields);            
        }

        public override string ToString()
        {
            string ap = "";
            bool skipFirstField = false;

            if (root is ParameterHeapVertex)
                ap += (root as ParameterHeapVertex).name;
            else if (root is GlobalLoadVertex)
                ap += "";
            else if (root is ReturnVertex)
            {
                ap += "ret";
                skipFirstField = true;
            }
            else if (root is ExceptionVertex)
            {
                ap += "throw";
                skipFirstField = true;
            }
            else
                ap += root.ToString();

            foreach (var field in fields)
            {
                if (skipFirstField)
                {
                    skipFirstField = false;
                    continue;
                }
                if (field is NamedField)
                {
                    if (root is GlobalLoadVertex
                        && fields.First().Equals(field))
                    {
                        //remove dll name
                        var name  = (field as NamedField).ToString();                        
                        ap += PhxUtil.RemoveAssemblyName(name);                                                
                    }
                    else
                        ap += "." + (field as NamedField).GetFieldName();
                }
                else if (field is RangeField)
                    ap += "[*]";
                else if (field is NullField)
                {
                    ap += "*";
                }
                else
                    ap += field.ToString();
            }
            return ap;
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var fieldlist = new List<FieldWrapper>();
            foreach(var field in fields)
                fieldlist.Add(new FieldWrapper(field));
            info.AddValue("fieldlist", fieldlist);
            info.AddValue("root", new HeapNodeWrapper(root));
        }

        #endregion
    }

    /// <summary>
    /// This class is used to compare fields instead of field.Equals() for legacy reasons    
    /// </summary>
    class WitnessComparer : IEqualityComparer<Field>
    {
        public bool Equals(Field x, Field y)
        {
            if(x.Equals(y))
                return true;

            if (x is NamedField && y is NamedField)
            {
                var n1 = (x as NamedField).GetFieldName();
                var n2 = (y as NamedField).GetFieldName();
                if (n1.Equals(n2))
                    return true;
            }
            return false;
        }

        public int GetHashCode(Field obj)
        {
            if (obj is NamedField)
                return (obj as NamedField).GetFieldName().GetHashCode();
            return obj.GetHashCode();
        }
    }
}
