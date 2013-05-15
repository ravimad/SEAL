using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SafetyAnalysis.Util;
namespace SafetyAnalysis.Framework.Graphs
{
    using NamedFieldKey = Pair<string, string>;
    public abstract class Field
    {
        private static int GUID = 7;
        private int Id;
        public string EnclosingTypename { get; private set; }

        public Field(string tname)
        {
            this.Id = GUID++;
            EnclosingTypename = tname;
        }
        
        public override bool Equals(object obj)
        {
            return (this == obj);
        }
        
        public override int GetHashCode()
        {
            return this.Id;
        }

        public virtual void GetObjectData(List<Pair<string, Object>> info)
        {
            if (this.EnclosingTypename != null)
                info.Add(new Pair<string, Object>("enclosingtype", EnclosingTypename));
        }

        /// <summary>
        /// Matches a wildcard field to any field.
        /// Used only in the case of summary application and field reads
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual bool EqualsModWildCard(Object obj)
        {            
            return this.Equals(obj);
        }
    }

    public class NullField : Field
    {
        private static NullField nullField = null;

        public static NullField Instance
        {
            get
            {
                if (nullField == null)
                    nullField = new NullField();
                return nullField;
            }
            private set { }
        }

        private NullField() : base(String.Empty)
        {
        }

        public static NullField Create(List<Pair<string, Object>> info)
        {
            return Instance;
        }

        public override void GetObjectData(List<Pair<string, object>> info)
        {
        }

        public override string ToString()
        {
            return String.Empty;
        }
    }    
    
    public class NamedField : Field
    {   
        //object pool     
        private static Dictionary<NamedFieldKey, NamedField> FieldTable
            = new Dictionary<NamedFieldKey, NamedField>();

        //A wild card field that matches any field 
        public static NamedField wildcard = NamedField.New("#", null);

        public string Name;                        

        public static NamedField New(string name, string encltype)
        {
            if (encltype == null)
                encltype = String.Empty;            

            var key = new NamedFieldKey(name, encltype);
            if (FieldTable.ContainsKey(key))
                return FieldTable[key];
            else
            {
                var nmfield = new NamedField(name, encltype);
                FieldTable.Add(key, nmfield);
                return nmfield;
            }
        }

        public static NamedField Create(List<Pair<string, Object>> info)
        {
            var pair = info[0];
            if (!pair.Key.Equals("name"))
                throw new NotSupportedException("missing property name");
            var name = (string)pair.Value;

            pair = info[1];
            if (!pair.Key.Equals("enclosingtype"))
                throw new NotSupportedException("extraneous property name: " + pair.Key);
            var encltype = (string)pair.Value;
            return NamedField.New(name, encltype);
        }        

        private NamedField(string name, string encltype)
            : base(encltype)
        {
            this.Name = name;
        }        

        public string GetQualifiedName()
        {
            return this.EnclosingTypename+"::"+Name;
        }

        public string GetFieldName()
        {
            return Name;
        }

        public override string ToString()
        {
            return this.EnclosingTypename+"::"+Name;
        }

        public override void GetObjectData(List<Pair<string, object>> info)
        {            
            info.Add(new Pair<string, Object>("name", this.Name));
            base.GetObjectData(info);
        }

        public override bool EqualsModWildCard(Object obj)
        {
            if (this == wildcard || obj == wildcard)
                return true;
            return this.Equals(obj);
        }
    }

    public class RangeField : Field
    {
        private static RangeField _defaultInstance = new RangeField();

        private RangeField()
            : base(String.Empty)
        {
        }

        public static RangeField GetInstance()
        {
            return _defaultInstance;
        }

        public static RangeField Create(List<Pair<string, Object>> info)
        {
            return GetInstance();
        }

        public override void GetObjectData(List<Pair<string, object>> info)
        {
        }
    }    
    
    public abstract class DelegateField : Field
    {
        public DelegateField() : base(String.Empty) { }
    }

    public class DelegateMethodField : DelegateField
    {
        private static DelegateMethodField _defaultInstance = new DelegateMethodField(); 

        private DelegateMethodField()
        {
        }

        public static DelegateMethodField GetInstance()
        {
            return _defaultInstance;
        }

        public static DelegateMethodField Create(List<Pair<string, Object>> info)
        {
            return GetInstance();
        }           

        public override void GetObjectData(List<Pair<string, object>> info)
        {            
        }
    }

    public class DelegateRecvrField : DelegateField
    {
        private static DelegateRecvrField _defaultInstance = new DelegateRecvrField();

        private DelegateRecvrField()         
        {
        }

        public static DelegateRecvrField GetInstance()
        {
            return _defaultInstance;
        }

        public static DelegateRecvrField Create(List<Pair<string, Object>> info)
        {
            return GetInstance();
        }
      
        public override void GetObjectData(List<Pair<string, object>> info)
        {            
        }
    }     
}
