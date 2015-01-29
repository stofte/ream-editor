using Castle.MicroKernel.Lifestyle.Scoped;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Repository
{
    // http://docs.castleproject.org/(S(kwaa14uzdj55gv55dzgf0vui))/Windsor.Implementing%20custom%20scope.ashx
    public class EditorScopeAccessor : IScopeAccessor
    {
        private static readonly ConcurrentDictionary<Guid, ILifetimeScope> collection = new ConcurrentDictionary<Guid, ILifetimeScope>();  

        public ILifetimeScope GetScope(Castle.MicroKernel.Context.CreationContext context)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
