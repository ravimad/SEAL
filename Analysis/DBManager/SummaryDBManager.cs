using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Purity
{
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Data;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using System.Linq.Expressions;
    using System.ComponentModel;
    using System;


    [global::System.Data.Linq.Mapping.DatabaseAttribute(Name = "PurityDB")]
    public partial class PurityDBDataContext : System.Data.Linq.DataContext
    {

        private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();

        #region Extensibility Method Definitions
        partial void OnCreated();
        partial void Insertpuritysummary(puritysummary instance);
        partial void Updatepuritysummary(puritysummary instance);
        partial void Deletepuritysummary(puritysummary instance);
        partial void InsertTypeHierarchy(TypeHierarchy instance);
        partial void UpdateTypeHierarchy(TypeHierarchy instance);
        partial void DeleteTypeHierarchy(TypeHierarchy instance);
        #endregion        

        public PurityDBDataContext(string connection) :
            base(connection, mappingSource)
        {
            OnCreated();
        }

        public PurityDBDataContext(System.Data.IDbConnection connection) :
            base(connection, mappingSource)
        {
            OnCreated();
        }

        public PurityDBDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) :
            base(connection, mappingSource)
        {
            OnCreated();
        }

        public PurityDBDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) :
            base(connection, mappingSource)
        {
            OnCreated();
        }

        public System.Data.Linq.Table<puritysummary> puritysummaries
        {
            get
            {
                return this.GetTable<puritysummary>();
            }
        }

        public System.Data.Linq.Table<TypeHierarchy> TypeHierarchies
        {
            get
            {
                return this.GetTable<TypeHierarchy>();
            }
        }

        public System.Data.Linq.Table<MethodInfo> MethodInfos
		{
			get
			{
				return this.GetTable<MethodInfo>();
			}
		}
		
		public System.Data.Linq.Table<TypeInfo> TypeInfos
		{
			get
			{
				return this.GetTable<TypeInfo>();
			}
		}	
    }

    [global::System.Data.Linq.Mapping.TableAttribute(Name = "puritysummaries")]
    public partial class puritysummary : INotifyPropertyChanging, INotifyPropertyChanged
    {

        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);

        private int _ID;

        private string _typename;

        private string _methodname;

        private string _methodSignature;

        private System.Data.Linq.Binary _purityData;

        private string _dllname;

        #region Extensibility Method Definitions
        partial void OnLoaded();
        partial void OnValidate(System.Data.Linq.ChangeAction action);
        partial void OnCreated();
        partial void OnIDChanging(int value);
        partial void OnIDChanged();
        partial void OntypenameChanging(string value);
        partial void OntypenameChanged();
        partial void OnmethodnameChanging(string value);
        partial void OnmethodnameChanged();
        partial void OnmethodSignatureChanging(string value);
        partial void OnmethodSignatureChanged();
        partial void OnpurityDataChanging(System.Data.Linq.Binary value);
        partial void OnpurityDataChanged();
        partial void OndllnameChanging(string value);
        partial void OndllnameChanged();
        #endregion

        public puritysummary()
        {
            OnCreated();
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_ID", DbType = "Int NOT NULL IDENTITY", IsDbGenerated = true, IsPrimaryKey = true)]        
        public int ID
        {            
            get
            {                
                return this._ID;
            }
            set
            {
                if ((this._ID != value))
                {
                    this.OnIDChanging(value);
                    this.SendPropertyChanging();
                    this._ID = value;
                    this.SendPropertyChanged("ID");
                    this.OnIDChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_typename", DbType = "NVarChar(4000) NOT NULL", CanBeNull = false, IsPrimaryKey = false)]
        public string typename
        {
            get
            {
                return this._typename;
            }
            set
            {
                if ((this._typename != value))
                {
                    this.OntypenameChanging(value);
                    this.SendPropertyChanging();
                    this._typename = value;
                    this.SendPropertyChanged("typename");
                    this.OntypenameChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_methodname", DbType = "NVarChar(4000) NOT NULL", CanBeNull = false, IsPrimaryKey = false)]
        public string methodname
        {
            get
            {
                return this._methodname;
            }
            set
            {
                if ((this._methodname != value))
                {
                    this.OnmethodnameChanging(value);
                    this.SendPropertyChanging();
                    this._methodname = value;
                    this.SendPropertyChanged("methodname");
                    this.OnmethodnameChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_methodSignature", DbType = "NVarChar(4000) NOT NULL", CanBeNull = false, IsPrimaryKey = false)]
        public string methodSignature
        {
            get
            {
                return this._methodSignature;
            }
            set
            {
                if ((this._methodSignature != value))
                {
                    this.OnmethodSignatureChanging(value);
                    this.SendPropertyChanging();
                    this._methodSignature = value;
                    this.SendPropertyChanged("methodSignature");
                    this.OnmethodSignatureChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_purityData", 
            DbType ="Image", CanBeNull = true, UpdateCheck = UpdateCheck.Never)]
        public System.Data.Linq.Binary purityData
        {
            get
            {
                return this._purityData;
            }
            set
            {
                if ((this._purityData != value))
                {
                    this.OnpurityDataChanging(value);
                    this.SendPropertyChanging();
                    this._purityData = value;
                    this.SendPropertyChanged("purityData");
                    this.OnpurityDataChanged();
               }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_dllname", DbType = "NVarChar(4000)", CanBeNull = true, UpdateCheck = UpdateCheck.Never)]
        public string dllname
        {
            get
            {
                return this._dllname;
            }
            set
            {
                if ((this._dllname != value))
                {
                    this.OndllnameChanging(value);
                    this.SendPropertyChanging();
                    this._dllname = value;
                    this.SendPropertyChanged("dllname");
                    this.OndllnameChanged();
                }
            }
        }

        public event PropertyChangingEventHandler PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;        

        protected virtual void SendPropertyChanging()
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, emptyChangingEventArgs);
            }
        }

        protected virtual void SendPropertyChanged(String propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [global::System.Data.Linq.Mapping.TableAttribute(Name = "TypeHierarchy")]
    public partial class TypeHierarchy : INotifyPropertyChanging, INotifyPropertyChanged
    {

        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);

        private string _Typename;

        private string _SuperTypename;

        private string _dllname;

        #region Extensibility Method Definitions
        partial void OnLoaded();
        partial void OnValidate(System.Data.Linq.ChangeAction action);
        partial void OnCreated();
        partial void OnTypenameChanging(string value);
        partial void OnTypenameChanged();
        partial void OnSuperTypenameChanging(string value);
        partial void OnSuperTypenameChanged();
        partial void OndllnameChanging(string value);
        partial void OndllnameChanged();
        #endregion

        public TypeHierarchy()
        {
            OnCreated();
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Typename", DbType = "NVarChar(4000) NOT NULL", CanBeNull = false, IsPrimaryKey = true)]
        public string Typename
        {
            get
            {
                return this._Typename;
            }
            set
            {
                if ((this._Typename != value))
                {
                    this.OnTypenameChanging(value);
                    this.SendPropertyChanging();
                    this._Typename = value;
                    this.SendPropertyChanged("Typename");
                    this.OnTypenameChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_SuperTypename", DbType = "NVarChar(4000) NOT NULL", CanBeNull = false, IsPrimaryKey = true)]
        public string SuperTypename
        {
            get
            {
                return this._SuperTypename;
            }
            set
            {
                if ((this._SuperTypename != value))
                {
                    this.OnSuperTypenameChanging(value);
                    this.SendPropertyChanging();
                    this._SuperTypename = value;
                    this.SendPropertyChanged("SuperTypename");
                    this.OnSuperTypenameChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_dllname", DbType = "NVarChar(4000) NOT NULL", CanBeNull = true, IsPrimaryKey = false)]
        public string dllname
        {
            get
            {
                return this._dllname;
            }
            set
            {
                if ((this._dllname != value))
                {
                    this.OndllnameChanging(value);
                    this.SendPropertyChanging();
                    this._dllname = value;
                    this.SendPropertyChanged("dllname");
                    this.OndllnameChanged();
                }
            }
        }
        public event PropertyChangingEventHandler PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SendPropertyChanging()
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, emptyChangingEventArgs);
            }
        }

        protected virtual void SendPropertyChanged(String propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [global::System.Data.Linq.Mapping.TableAttribute(Name = "MethodInfo")]
    public partial class MethodInfo : INotifyPropertyChanging, INotifyPropertyChanged
    {

        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);

        private string _typename;

        private string _methodname;

        private string _methodSignature;

        private bool _IsVirtual;

        private bool _IsAbstract;

        private bool _IsInstance;

        private int _ID;

        private string _dllname;

        #region Extensibility Method Definitions
        partial void OnLoaded();
        partial void OnValidate(System.Data.Linq.ChangeAction action);
        partial void OnCreated();
        partial void OntypenameChanging(string value);
        partial void OntypenameChanged();
        partial void OnmethodnameChanging(string value);
        partial void OnmethodnameChanged();
        partial void OnmethodSignatureChanging(string value);
        partial void OnmethodSignatureChanged();
        partial void OnIsVirtualChanging(bool value);
        partial void OnIsVirtualChanged();
        partial void OnIsAbstractChanging(bool value);
        partial void OnIsAbstractChanged();
        partial void OnIsInstanceChanging(bool value);
        partial void OnIsInstanceChanged();
        partial void OnIDChanging(int value);
        partial void OnIDChanged();
        partial void OndllnameChanging(string value);
        partial void OndllnameChanged();
        #endregion

        public MethodInfo()
        {
            OnCreated();
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_typename", DbType = "NVarChar(200) NOT NULL", CanBeNull = false)]
        public string typename
        {
            get
            {
                return this._typename;
            }
            set
            {
                if ((this._typename != value))
                {
                    this.OntypenameChanging(value);
                    this.SendPropertyChanging();
                    this._typename = value;
                    this.SendPropertyChanged("typename");
                    this.OntypenameChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_methodname", DbType = "NVarChar(200) NOT NULL", CanBeNull = false)]
        public string methodname
        {
            get
            {
                return this._methodname;
            }
            set
            {
                if ((this._methodname != value))
                {
                    this.OnmethodnameChanging(value);
                    this.SendPropertyChanging();
                    this._methodname = value;
                    this.SendPropertyChanged("methodname");
                    this.OnmethodnameChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_methodSignature", DbType = "NVarChar(4000) NOT NULL", CanBeNull = false)]
        public string methodSignature
        {
            get
            {
                return this._methodSignature;
            }
            set
            {
                if ((this._methodSignature != value))
                {
                    this.OnmethodSignatureChanging(value);
                    this.SendPropertyChanging();
                    this._methodSignature = value;
                    this.SendPropertyChanged("methodSignature");
                    this.OnmethodSignatureChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_IsVirtual", DbType = "Bit NOT NULL")]
        public bool IsVirtual
        {
            get
            {
                return this._IsVirtual;
            }
            set
            {
                if ((this._IsVirtual != value))
                {
                    this.OnIsVirtualChanging(value);
                    this.SendPropertyChanging();
                    this._IsVirtual = value;
                    this.SendPropertyChanged("IsVirtual");
                    this.OnIsVirtualChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_IsAbstract", DbType = "Bit NOT NULL")]
        public bool IsAbstract
        {
            get
            {
                return this._IsAbstract;
            }
            set
            {
                if ((this._IsAbstract != value))
                {
                    this.OnIsAbstractChanging(value);
                    this.SendPropertyChanging();
                    this._IsAbstract = value;
                    this.SendPropertyChanged("IsAbstract");
                    this.OnIsAbstractChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_IsInstance", DbType = "Bit NOT NULL")]
        public bool IsInstance
        {
            get
            {
                return this._IsInstance;
            }
            set
            {
                if ((this._IsInstance != value))
                {
                    this.OnIsInstanceChanging(value);
                    this.SendPropertyChanging();
                    this._IsInstance = value;
                    this.SendPropertyChanged("IsInstance");
                    this.OnIsInstanceChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_ID", AutoSync = AutoSync.OnInsert, DbType = "Int NOT NULL IDENTITY", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID
        {
            get
            {
                return this._ID;
            }
            set
            {
                if ((this._ID != value))
                {
                    this.OnIDChanging(value);
                    this.SendPropertyChanging();
                    this._ID = value;
                    this.SendPropertyChanged("ID");
                    this.OnIDChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_dllname", DbType = "NVarChar(200)")]
        public string dllname
        {
            get
            {
                return this._dllname;
            }
            set
            {
                if ((this._dllname != value))
                {
                    this.OndllnameChanging(value);
                    this.SendPropertyChanging();
                    this._dllname = value;
                    this.SendPropertyChanged("dllname");
                    this.OndllnameChanged();
                }
            }
        }

        public event PropertyChangingEventHandler PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SendPropertyChanging()
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, emptyChangingEventArgs);
            }
        }

        protected virtual void SendPropertyChanged(String propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [global::System.Data.Linq.Mapping.TableAttribute(Name = "TypeInfo")]
    public partial class TypeInfo : INotifyPropertyChanging, INotifyPropertyChanged
    {

        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);

        private string _typename;

        private bool _IsInterface;

        private string _dllname;

        #region Extensibility Method Definitions
        partial void OnLoaded();
        partial void OnValidate(System.Data.Linq.ChangeAction action);
        partial void OnCreated();
        partial void OntypenameChanging(string value);
        partial void OntypenameChanged();
        partial void OnIsInterfaceChanging(bool value);
        partial void OnIsInterfaceChanged();
        partial void OndllnameChanging(string value);
        partial void OndllnameChanged();
        #endregion

        public TypeInfo()
        {
            OnCreated();
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_typename", DbType = "NVarChar(200) NOT NULL", CanBeNull = false, IsPrimaryKey = true)]
        public string typename
        {
            get
            {
                return this._typename;
            }
            set
            {
                if ((this._typename != value))
                {
                    this.OntypenameChanging(value);
                    this.SendPropertyChanging();
                    this._typename = value;
                    this.SendPropertyChanged("typename");
                    this.OntypenameChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_IsInterface", DbType = "Bit NOT NULL")]
        public bool IsInterface
        {
            get
            {
                return this._IsInterface;
            }
            set
            {
                if ((this._IsInterface != value))
                {
                    this.OnIsInterfaceChanging(value);
                    this.SendPropertyChanging();
                    this._IsInterface = value;
                    this.SendPropertyChanged("IsInterface");
                    this.OnIsInterfaceChanged();
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_dllname", DbType = "NVarChar(200)")]
        public string dllname
        {
            get
            {
                return this._dllname;
            }
            set
            {
                if ((this._dllname != value))
                {
                    this.OndllnameChanging(value);
                    this.SendPropertyChanging();
                    this._dllname = value;
                    this.SendPropertyChanged("dllname");
                    this.OndllnameChanged();
                }
            }
        }

        public event PropertyChangingEventHandler PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SendPropertyChanging()
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, emptyChangingEventArgs);
            }
        }

        protected virtual void SendPropertyChanged(String propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

