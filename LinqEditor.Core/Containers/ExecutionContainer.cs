using LinqEditor.Core.Generated;
using LinqEditor.Core.Models.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Containers
{
    public abstract class ExecutionContainer : MarshalByRefObject
    {
        /// <summary>
        /// The base assembly of the container (fx schema assembly)
        /// </summary>
        protected Assembly _baseAssembly;
        protected ProgramType _runnerType;

        protected void InitializeAppDomain()
        {
            // i dont get how the runtime actually loads assemblies, so for now, we just grab
            // the resolve failed event, and return the passed assembly straight. this avoid
            // having to use the probe-path configuration setup, and lets us persist assemblies
            // where ever
            AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
            {
                if (args.RequestingAssembly != null)
                {
                    return _baseAssembly;
                }
                return null;
            };
        }

        // http://blogs.microsoft.co.il/sasha/2008/07/19/appdomains-and-remoting-life-time-service/
        public override object InitializeLifetimeService()
        {
            return null;
        }

        protected async Task<ExecuteResult> AsyncLauncher(Func<ExecuteResult> func)
        {
            return await Task.Run(func);
        }
    }
}
