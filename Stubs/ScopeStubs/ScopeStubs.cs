using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

namespace ScopeStubs
{
    public class Schema
    {
        // Properties
        internal int Capacity { get; set; }
        private List<ScopeRuntime.ColumnInfo> columns = new List<ScopeRuntime.ColumnInfo>();
        public IEnumerable<ScopeRuntime.ColumnInfo> Columns { get { return columns; } }
        
        // Methods
        public Schema()
        {
        }
        public Schema(string[] schema)
        {
        }
        public Schema(string schema)
        {
        }

        public void Add(ScopeRuntime.ColumnInfo columnInfo)
        {
            columns.Add(columnInfo);
        }

        public Schema Clone()
        {
            return this;
        }
        public Schema CloneWithSource()
        {
            return this;
        }

        public bool Contains(string name)
        {
            return true;
        }
        internal void CopyPropertiesTo(Schema clone)
        {            
        }
        internal Schema DeepCopy(string tableName)
        {
            return this;
        }
        public string GetString()
        {
            return String.Empty;
        }
        public string GetString(bool haveTypes)
        {
            return String.Empty;
        }
        public int IndexOf(string name)
        {
            return 0;
        }        
        public void Merge(Schema delta)
        {
        }
        public Schema PassThrough()
        {
            return this;
        }
        public void SetTable(string table)
        {
        }

        public override string ToString()
        {
            return String.Empty;
        }
    }
    public class ColumnData
    {
        // Fields
        internal const int NULLHASH = 0x32e56baf;

        // Methods
        protected ColumnData() { }
        public ColumnData Clone()
        {
            return this;
        }
        public int Compare(ColumnData data)
        {
            return 0;
        }
        public  void Copy(ColumnData destination)
        {
        }
        public  void CopyTo(ColumnData destination)
        {
        }
        public void Deserialize(BinaryReader reader) { }
        public void Deserialize(string token) { }
        public void DeserializeByteSortable(BinaryReader reader) { }
        public void DeserializeByteSortable(byte[] buffer, ref int offset, bool complement) { }
        private void FailGet(ScopeRuntime.ColumnDataType targetType) { }
        private void FailSet(ScopeRuntime.ColumnDataType sourceType) { }
        internal static int Hash(bool a) { return 0; }
        internal static int Hash(uint a) { return 0; }
        internal static int Hash(ulong a) { return 0; }
        public  bool IsNull() { return true; }
        public void Reset() { }
        public void Serialize(BinaryWriter writer) { }
        public void Serialize(StreamWriter writer) { }
        public void SerializeAsByteSortable(BinaryWriter writer) { }
        public void SerializeAsByteSortable(byte[] buffer, ref int offset) { }
        public void Set(Object o) { }
        public void SetNull() { }
        public void UnsafeSet(Object o) { }
        
        // Properties
        public byte[] Binary { get; set; }
        public bool Boolean { get; set; }
        public bool? BooleanQ { get; set; }
        public byte Byte { get; set; }
        public byte? ByteQ { get; set; }
        public string CLRType { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime? DateTimeQ { get; set; }
        public decimal Decimal { get; set;  }
        public decimal? DecimalQ { get; set; }
        public double Double { get; set;  }
        public double? DoubleQ { get; set; }
        public float Float { get; set;  }
        public float? FloatQ { get; set;  }
        public int Integer { get; set; }
        public int? IntegerQ { get; set; }
        public long Long { get; set; }
        public long? LongQ { get; set; }
        public sbyte SByte { get; set; }
        public sbyte? SByteQ { get; set; }
        public short Short { get; set; }
        public short? ShortQ { get; set; }
        public int Size { get; set; }
        public string String { get; set; }
        public ScopeRuntime.ColumnDataType Type { get; set;  }
        public uint UInt { get; set;  }
        public uint? UIntQ { get; set;  }
        public ulong ULong { get; set;  }
        public ulong? ULongQ { get; set;  }
        public ScopeRuntime.ScopeUrl Url { get; set;  }
        public ushort UShort { get; set;  }
        public ushort? UShortQ { get; set; }
        public object Value { get; set;  }
        public Type ValueType { get; set;  }
    }
    
    public class Row
    {
        public ScopeRuntime.ColumnData[] Columns { get; set;  }
        public int Count { get; set;  }
        public ScopeRuntime.Schema Schema { get; set;  }
        public int Size { get; set;  } 

        public Row Clone()
        {
            return this;
        }
        public void Copy(Row destination)
        {
            this.Columns = destination.Columns;
        }        

        public void CopyTo(Row destination)
        {
            this.Columns = destination.Columns;
        }

        public void Deserialize(BinaryReader reader)
        {
        }

        public bool Deserialize(StreamReader reader, char delimiter)
        {
            return true;
        }
        public static void DeserializeRow(Row row, BinaryReader reader)
        {
        }
        internal static char[] EscapeString(string s, string delimiter, bool escape, out int resultSize)
        {
            resultSize = 0;
            return null;
        }
        public void Reset()
        {
        }
        public void Serialize(BinaryWriter writer)
        {
        }
        public void Serialize(StreamWriter writer)
        {
        }
        public void Serialize(StreamWriter writer, string delimiter, bool doubleToFloat, bool escape)
        {
        }
        public static void SerializeRow(Row row, BinaryWriter writer)
        {
        }      
    }

    public class RowSet
    {        
        public ScopeRuntime.Row row;        
        public IEnumerable<ScopeRuntime.Row> get_Rows()
        {
            return new RowEnum(this);
        }

        public static RowSet Create(IEnumerable<ScopeRuntime.Row> list)
        {
            var rowset = new RowSet();
            foreach (var elem in list)
            {
                rowset.Add(elem);
            }
            return rowset;
        }

        public void Add(ScopeRuntime.Row r)
        {
            //this will be a weak update as 'row' is a field
            row = r;
        }

        class RowEnum : IEnumerable<ScopeRuntime.Row>, IEnumerator<ScopeRuntime.Row>
        {
            public RowSet rs;

            public RowEnum(RowSet rset)
            {
                rs = rset;
            }

            public IEnumerator<ScopeRuntime.Row> GetEnumerator()
            {
                return this;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this;   
            }

            #region IEnumerator<Row> Members

            public ScopeRuntime.Row Current
            {
                get { return rs.row; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {                
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current
            {
                get { return rs.row; }
            }

            public bool MoveNext()
            {
                return false;
            }

            public void Reset()
            {                
            }

            #endregion
        }
    }    


}
