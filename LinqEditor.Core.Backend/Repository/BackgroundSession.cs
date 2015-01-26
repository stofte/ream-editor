using LinqEditor.Core.Backend.Models;
using LinqEditor.Core.Backend.Settings;
using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.Schema.Services;
using LinqEditor.Core.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Repository
{
    // ok to use Task.Run when "just" offloading from UI thread
    // http://blogs.msdn.com/b/pfxteam/archive/2012/04/12/10293335.aspx?Redirected=true
    public class BackgroundSession : Session, IBackgroundSession
    {
        public BackgroundSession(ICSharpCompiler compiler, ISqlSchemaProvider schemaProvider, ITemplateService generator, IUserSettings userSettings) :
            base(compiler, schemaProvider, generator, userSettings) { }

        public async Task<InitializeResult> InitializeAsync(string connectionString)
        {
            return await Task.Run(() => Initialize(connectionString));
        }

        public async Task<ExecuteResult> ExecuteAsync(string sourceFragment)
        {
            return await Task.Run(() => Execute(sourceFragment));
        }
    }
}
