using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Containers
{
    public interface IContainerMapper
    {
        Guid MapConnectionString(string connectionString);
        Guid CodeContainer { get; }
    }
}
