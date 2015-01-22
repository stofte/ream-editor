using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqEditor.Utility.Helpers;

namespace LinqEditor.Utility.Test
{
    [TestClass]
    public class TypeHelperTests
    {
        [TestMethod]
        public void Can_Map_Type_As_DataTable()
        {
            var foo = new { Foo = 1, Bar = 1 };
            var table = foo.GetType().GetTypeSchemaAsDataTable();

        }
    }
}
