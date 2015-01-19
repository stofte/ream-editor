using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LinqEditor.Backend.Test
{
    [TestClass]
    public class DumperTests
    {
        public class Foo {
            public int Id;
            public string Description;
            public string TestGetSet { get; set; }
        }

        private IEnumerable<Foo> GetIEnumerableFoo()
        {
            var l = new List<Foo>()
            {
                new Foo { Id = 1, Description = "Foo 1 "},
                new Foo { Id = 2, Description = "Foo 2 "}
                //,
                //new Foo { Id = 3, Description = "Foo 3 "},
                //new Foo { Id = 4, Description = "Foo 4 "},
                //new Foo { Id = 5, Description = "Foo 5 "},
                //new Foo { Id = 6, Description = "Foo 6 "}
            };

            return l.AsEnumerable();
        }

        [TestMethod]
        public void Can_Dump_IEnumerable()
        {
            //var l = GetIEnumerableFoo();
            //Dumper.Result = new QueryResult();
            //l.Dump();//Where(t => t.Id % 2 == 0).Select(x => new { Desc = x.Description }).Dump();
            //var res = Dumper.Result.Get();
        }
    }
}
