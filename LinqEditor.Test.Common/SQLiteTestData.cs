using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Test.Common
{
    public static class SQLiteTestData
    {
        public const string Connstr1 = @"data source=C:\foo.sqlite;";
        public const string Connstr2 = @"data source=\\some.where\some\folder\data.db;";
        public const string Connstr3 = @"data source=C:\foo.sqlite;Version=3;";
    }
}
