using Castle.MicroKernel.Lifestyle.Scoped;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Common.Scopes
{
    // http://docs.castleproject.org/(S(kwaa14uzdj55gv55dzgf0vui))/Windsor.Implementing%20custom%20scope.ashx
    // http://stackoverflow.com/questions/26096502/argument-bound-lifestyle-in-castle-windsor
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