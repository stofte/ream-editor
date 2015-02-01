using IQToolkit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Common.Generated
{
    /// <summary>
    /// Dynamic query class implements this interface to make working with it easier.
    /// </summary>
    public interface IDatabaseProgram
    {
        IEnumerable<DataTable> Execute(IEntityProvider entityProvider);
    }
}
