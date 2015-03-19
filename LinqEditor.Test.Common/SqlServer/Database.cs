using LinqEditor.Core;
using System;
using System.Data.SqlClient;
using System.IO;

namespace LinqEditor.Test.Common.SqlServer
{
    public class Database : IDisposable
    {
        private string LocalDbMaster 
        { 
            get 
            {
                if (Environment.GetEnvironmentVariable("CI") == "True")
                {
                    return @"Server=(local)\SQL2014;Initial Catalog=master;Integrated Security=True";
                }
                return @"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True"; 
            } 
        }

        private readonly string _databaseName;
        private readonly string _fileName;
        private readonly string _schema;
        private readonly string _script;
        public readonly string ConnectionString;

        public Database(string databaseName, string schema = null, string script = null)
        {
            _databaseName = databaseName;
            _fileName = Path.GetFullPath(databaseName);
            _schema = schema ?? DefaultSchema();
            _script = script ?? DefaultScript();
            ConnectionString = string.Format("Data Source=(LocalDB)\\v11.0;Initial Catalog={0};Integrated Security=True;" +
                "MultipleActiveResultSets=True;AttachDBFilename={1}.mdf", _databaseName, _fileName);
            CreateDatabase();
        }

        public void Dispose()
        {
            DetachDatabase();

            if (File.Exists(_fileName + ".mdf")) File.Delete(_fileName + ".mdf");
            if (File.Exists(_fileName + "_log.ldf")) File.Delete(_fileName + "_log.ldf");
        }

        public void RecreateTestData()
        {
            ExecuteQuery(_script);
        }

        public void ExecuteQuery(string script)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = script;
                cmd.ExecuteNonQuery();
            }
        }

        private bool DetachDatabase()
        {
            try
            {
                using (var connection = new SqlConnection(LocalDbMaster))
                {
                    connection.Open();
                    var cmd = connection.CreateCommand();
                    // http://stackoverflow.com/questions/16243828/how-to-detach-a-localdb-sql-server-express-file-in-code
                    cmd.CommandText = String.Format("alter database {0} set offline with rollback immediate", _databaseName);
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = String.Format("exec sp_detach_db '{0}', 'true'", _databaseName);
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void CreateDatabase()
        {
            Dispose();

            using (var connection = new SqlConnection(LocalDbMaster))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = string.Format("CREATE DATABASE {0} ON (NAME = N'{0}', FILENAME = '{1}.mdf')",
                    _databaseName,
                    _fileName);
                cmd.ExecuteNonQuery();
            }

            ExecuteQuery(_schema);
        }

        // looks for script in output folder
        private static string DefaultSchema()
        {
            var file = PathUtility.CurrentPath + @"SqlServer\schema.sql";
            return File.ReadAllText(file);
        }

        private static string DefaultScript()
        {
            var file = PathUtility.CurrentPath + @"SqlServer\script.sql";
            return File.ReadAllText(file);
        }
    }
}
