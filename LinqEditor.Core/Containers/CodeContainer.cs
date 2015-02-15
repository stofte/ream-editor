using LinqEditor.Core.Generated;
using LinqEditor.Core.Models.Editor;
using System;
using System.Reflection;

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
        
        public ExecuteResult Execute(string path)
        {
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
