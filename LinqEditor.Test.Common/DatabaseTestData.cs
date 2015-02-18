using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Test.Common
{
    public class DatabaseTestData
    {
        public const string Connstr1 = @"Data Source=.\sqlexpress;Integrated Security=True;Initial Catalog=Opera56100DB";
        public const string Connstr2 = @"Data Source=sql.somewhere,1437; UID=yyy; PWD=xxx; DATABASE=mydbname";
        public const string Invalid1 = @"not a connection string";
    }
}
