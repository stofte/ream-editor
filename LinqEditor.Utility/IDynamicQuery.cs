using IQToolkit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Utility
{
    /// <summary>
    /// Dynamic query class implements this interface to make working with it easier.
    /// </summary>
    public interface IDynamicQuery
    {
        IEnumerable<DataTable> Execute(IEntityProvider entityProvider);
    }
}
