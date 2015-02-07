using IQToolkit;
using System.Collections.Generic;
using System.Data;

namespace LinqEditor.Core.Generated
{
    /// <summary>
    /// Dynamic query class implements this interface to make working with it easier.
    /// </summary>
    public interface IDatabaseProgram
    {
        string ConnectionString { get; }
        IEnumerable<DataTable> Execute(IEntityProvider entityProvider);
    }
}
