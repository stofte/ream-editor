using IQToolkit;
using LinqEditor.Backend.Dumper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Backend.Interfaces
{
    public interface IQueryUnit
    {
        IEnumerable<DataTable> Execute(IEntityProvider entityProvider);
    }

    public interface IDatabase
    {
        int Execute();
    }
}
