using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Test.Common.SQLite
{
    public static class Scripts
    {
        public const string Schema = @"
create table Foo (
    Id int PRIMARY KEY,
    Description Text
);
";

        public const string Data = @"
delete from Foo;
insert into Foo(Id,Description) values(0, 'Foo 0');
insert into Foo(Id,Description) values(1, 'Foo 1');
insert into Foo(Id,Description) values(2, 'Foo 2');
insert into Foo(Id,Description) values(3, 'Foo 3');
";
    }
}
