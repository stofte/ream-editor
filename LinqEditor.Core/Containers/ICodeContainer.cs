using LinqEditor.Core.Models.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Containers
{
    public interface ICodeContainer
    {
        LoadAppDomainResult Initialize();
        ExecuteResult Execute(byte[] assembly);
        ExecuteResult Execute(string path);
    }
}
