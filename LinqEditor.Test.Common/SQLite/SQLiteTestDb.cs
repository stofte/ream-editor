using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using LinqEditor.Core;

namespace LinqEditor.Test.Common.SQLite
{
    public class SQLiteTestDb : IDisposable
    {
        private readonly string _databaseName;
        private readonly string _fileName;
        private readonly string _schema;
        private readonly string _script;

        public string ConnectionString
        {
            get { return string.Format("Data Source={0};Version=3;", _fileName); }
        }

        public SQLiteTestDb(string databaseName, string schema = null, string script = null)
        {
            if (!databaseName.EndsWith(".sqlite"))
            {
                throw new ArgumentException("databaseName must be sqlite file");
            }

            _databaseName = databaseName;
            _schema = schema ?? DefaultSchema();
            _script = script ?? DefaultScript();
            _fileName = Path.GetFullPath(databaseName);
            SQLiteConnection.CreateFile(_fileName);

            // create schema
            ExecuteQuery(_schema);
        }

        public void RecreateTestData()
        {
            ExecuteQuery(_script);
        }

        public void ExecuteQuery(string script)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new SQLiteCommand(script, conn);
                var ret = cmd.ExecuteNonQuery();
            }
        }

        // looks for script in output folder
        private static string DefaultSchema()
        {
            var file = PathUtility.CurrentPath + @"SQLite\schema.sql";
            return File.ReadAllText(file);
        }

        private static string DefaultScript()
        {
            var file = PathUtility.CurrentPath + @"SQLite\script.sql";
            return File.ReadAllText(file);
        }

        public void Dispose()
        {
            if (File.Exists(_fileName))
            {
                File.Delete(_fileName);
            }
        }
    }
}
