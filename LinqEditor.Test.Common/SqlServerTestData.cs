using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Test.Common
{
    public class SqlServerTestData
    {
        public const string Connstr1 = @"Data Source=.\sqlexpress;Integrated Security=True;Initial Catalog=Opera56100DB";
        
        public const string Connstr2 = @"Data Source=sql.somewhere,1437; UID=yyy; PWD=xxx; DATABASE=mydbname";
        public const string Connstr3 = @"Data Source=.\sqlexpress;Integrated Security=True;Initial Catalog=AnotherDB";
        public const string Connstr4_No_Catalog = @"Data Source=some.server.net;Integrated Security=True";
        public const string Connstr5_No_Catalog = @"Data Source=some.other.server;Integrated Security=True";
        public const string Invalid1 = @"not a connection string";

        // also sets lower timeout to speed up test
        public const string NonExistingServerConnStr = @"Data Source=nowhere;Integrated Security=True;Initial Catalog=Opera56100DB;Timeout=3";
    }
}
