using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle.Scoped;
using LinqEditor.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Scopes
{
    public class ThreadSafeDefaultLifetimeScope : ILifetimeScope
    {
        private static readonly Action<Burden> emptyOnAfterCreated = delegate { };
        private readonly object @lock = new object();
        private readonly Action<Burden> onAfterCreated;
        private IScopeCache scopeCache;

        public ThreadSafeDefaultLifetimeScope(IScopeCache scopeCache = null, Action<Burden> onAfterCreated = null)
        {
            this.scopeCache = scopeCache ?? new ScopeCache();
            this.onAfterCreated = onAfterCreated ?? emptyOnAfterCreated;
        }

        public void Dispose()
        {
            lock (@lock)
            {
                DebugLogger.Log("ThreadSafeDefaultLifetimeScope.Dispose");
                if (scopeCache == null)
                {
                    return;
                }
                var disposableCache = scopeCache as IDisposable;
                if (disposableCache != null)
                {
                    disposableCache.Dispose();
                }
                scopeCache = null;
            }
        }

        public Burden GetCachedInstance(ComponentModel model, ScopedInstanceActivationCallback createInstance)
        {
            lock (@lock)
            {
                var burden = scopeCache[model];
                if (burden == null)
                {
                    scopeCache[model] = burden = createInstance(onAfterCreated);
                }
                return burden;
            }
        }
    }
}
