using LinqEditor.Core.Models;
using System.Threading.Tasks;

namespace LinqEditor.DataAccess
{
    public interface IAssemblyFileProvider
    {
        Task<string> GetSchemaPath(Connection connection);
    }
}
