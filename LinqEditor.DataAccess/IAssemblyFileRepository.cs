using LinqEditor.Core.Models;
using System.Threading.Tasks;

namespace LinqEditor.DataAccess
{
    public interface IAssemblyFileRepository
    {
        Task<string> GetAssemblyFilePath(Connection connection);
    }
}
