namespace QueryEngine.Handlers
{
    using Microsoft.AspNetCore.Http;
    using QueryEngine.Services;
    using QueryEngine.Models;

    public class DebugHandler : BaseHandler<string, QueryInput>
    {
        SchemaService _schemaService;
        CompileService _compileService;
        DatabaseContextService _dbContextService;

        public DebugHandler(RequestDelegate next, SchemaService service, CompileService compileService, DatabaseContextService dbContextService) : base(next) 
        {
            _schemaService = service;
            _compileService = compileService;
            _dbContextService = dbContextService;
        }

        protected override bool Handle(string path)
        {
            return path.Contains("/debug");
        }

        protected override string Execute(QueryInput input)
        {
            var t = _dbContextService.GetDatabaseContext(input.ConnectionString, input.ServerType);
            // var schema = _schemaService.GetSchemaSource(input.ConnectionString, "debug");
            // var transformed = _compileService.TransformSource(input.Text, schema, "debug");
            // var x = new List<DebugHandler>();
            return t.ToString();
        }
    }
}