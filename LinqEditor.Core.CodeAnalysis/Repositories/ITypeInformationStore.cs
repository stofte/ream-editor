using LinqEditor.Core.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Repositories
{
    public interface ITypeInformationStore
    {
        TypeInformation GetInformation(TypeInfo typeInfo);
    }
}
