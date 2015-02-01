using LinqEditor.Core.CodeAnalysis.Models;
using LinqEditor.Core.Context;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Repositories
{
    public class TypeInformationStore : ITypeInformationStore
    {
        IContext _context;

        public TypeInformationStore(IContext context)
        {
            _context = context;
            _context.ContextUpdated += Initialize;
        }

        private void Initialize(string assemblyPath, string assemblyNamespace)
        {

        }

        public TypeInformation GetInformation(TypeInfo typeInfo)
        {
            throw new NotImplementedException();
        }
    }
}
