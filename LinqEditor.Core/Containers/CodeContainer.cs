using LinqEditor.Core.Generated;
using LinqEditor.Core.Models.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LinqEditor.Core.Containers
{
    public class CodeContainer : ExecutionContainer, IContainer
    {
        public LoadAppDomainResult Initialize(byte[] assembly)
        {
            throw new NotImplementedException("Cannot initialize CodeContainer with assembly");
        }

        public LoadAppDomainResult Initialize(string assemblyPath)
        {
            throw new NotImplementedException("Cannot initialize CodeContainer with assembly");
        }

        public LoadAppDomainResult Initialize()
        {
            base.InitializeAppDomain();
            return new LoadAppDomainResult();
        }

        public ExecuteResult Execute(byte[] assembly)
        {
            return ExecuteInternal(assembly, null);
        }

        public ExecuteResult Execute(byte[] assembly, CancellationToken ct)
        {
            if (ct != CancellationToken.None)
            {
                if (!ct.CanBeCanceled)
                {
                    throw new Exception("foo");
                }

                var timer = new System.Timers.Timer(100);
                timer.Elapsed += delegate
                {
                    if (ct.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                };
                timer.AutoReset = true;
                timer.Enabled = true;
            }
            return ExecuteInternal(assembly, null);
        }

        public ExecuteResult Execute(string path)
        {
            return ExecuteInternal(null, path);
        }

        public ExecuteResult Execute(string path, CancellationToken ct)
        {
            if (ct != CancellationToken.None)
            {
                if (!ct.CanBeCanceled)
                {
                    throw new Exception("foo");
                }

                var timer = new System.Timers.Timer(100);
                timer.Elapsed += delegate
                {
                    if (ct.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                    
                };
                timer.Enabled = true;
            }
            return ExecuteInternal(null, path);
        }

        private ExecuteResult ExecuteInternal(byte[] assembly, string path)
        {
            try
            {
                var assm = !string.IsNullOrEmpty(path) ? Assembly.LoadFile(path) : Assembly.Load(assembly);
                var programType = assm.GetType(string.Format("{0}.Program", assm.GetName().Name));

                var instance = Activator.CreateInstance(programType) as ICodeProgram;
                var output = instance.Execute();
                return new ExecuteResult
                {
                    CodeOutput = output,
                    Success = true
                };
            }
            catch (Exception e)
            {
                return new ExecuteResult
                {
                    Exception = e,
                    Success = false
                };
            }
        }


    }
}
