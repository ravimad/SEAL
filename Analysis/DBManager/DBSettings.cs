using System;
using System.IO;
using System.Data.SqlServerCe;

namespace SafetyAnalysis.Purity.Properties
{
    internal sealed partial class DBSettings : global::System.Configuration.ApplicationSettingsBase
    {

        private static DBSettings defaultInstance = ((DBSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new DBSettings())));

        public static DBSettings Default
        {
            get
            {
                return defaultInstance;
            }
        }        

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]        
        public string FileDBConnectionString
        {
            get
            {
                string filename;
                PurityAnalysisPhase.properties.TryGetValue("dbfilename", out filename);   
                var filepath = PurityAnalysisPhase.sealHome + filename;

                //check if the database exists
                if (!File.Exists(filepath))
                {
                    //create a db if it does not exist                    
                    string connStr = "Data Source=" + filepath + ";Max Database Size=4091";
                    SqlCeEngine engine = new SqlCeEngine(connStr);
                    engine.CreateDatabase();
                    SqlCeConnection conn = null;
                    try
                    {
                        conn = new SqlCeConnection(connStr);
                        conn.Open();

                        //create a methodinfo table
                        SqlCeCommand cmd = conn.CreateCommand();
                        cmd.CommandText = "CREATE TABLE MethodInfo (typename nvarchar(200) NOT NULL," +
                            "methodname nvarchar(200) NOT NULL,methodSignature nvarchar(4000) NOT NULL," +
                            "IsVirtual bit,IsAbstract bit,IsInstance bit," +
                            "dllname nvarchar(200)," + "ID int IDENTITY," + "PRIMARY KEY (ID))";
                        cmd.ExecuteNonQuery();

                        //create a typeinfo table
                        cmd.CommandText = "CREATE TABLE TypeInfo (typename nvarchar(200)," +
                            "IsInterface bit," + "dllname nvarchar(200)," + "PRIMARY KEY (typename))";
                        cmd.ExecuteNonQuery();

                        //create a typehierarchy table
                        cmd.CommandText = "CREATE TABLE TypeHierarchy (typename nvarchar(200)," +
                            "SuperTypename nvarchar(200)," + "dllname nvarchar(200)," +
                            "PRIMARY KEY (typename,SuperTypename))";
                        cmd.ExecuteNonQuery();

                        //create a puritysummaries table
                        cmd.CommandText = "CREATE TABLE puritysummaries (typename nvarchar(200) NOT NULL," +
                            "methodname nvarchar(200) NOT NULL,methodSignature nvarchar(4000) NOT NULL," +
                            "purityData image," + "dllname nvarchar(200)," +
                            "ID int IDENTITY," + "PRIMARY KEY (ID))";
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (Exception e)
                    {
                        conn.Close();
                        File.Delete(filepath);
                        throw e;
                    }
                }

                //make it not readonly
                System.IO.File.SetAttributes(filepath, System.IO.FileAttributes.Normal);
                return "Data Source=" + filepath+";Max Database Size=4091";
            }
        }
    }
}
