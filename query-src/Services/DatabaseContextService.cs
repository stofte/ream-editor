namespace QueryEngine.Services
{
    using System;
    using System.Collections.Generic;
    using QueryEngine.Models;

    /// <summary>
    /// Provides EF7 based database contexts
    /// </summary>
    public class DatabaseContextService
    {
        SchemaService _schemaService;
        CompileService _compileService;
        IDictionary<string, CompileResult> _map;

        public DatabaseContextService(SchemaService schemaService, CompileService compileService)
        {
            _schemaService = schemaService;
            _compileService = compileService;
            _map = new Dictionary<string, CompileResult>();
        }

        /// <summary>
        /// Returns the database context for the given connection string, 
        /// optionally loading and compiling the types if missing.
        /// </summary>
        public CompileResult GetDatabaseContext(string connectionString)
        {
            if (!_map.ContainsKey(connectionString))
            {
                var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");
                var schemaSrc = _schemaService.GetSchemaSource(connectionString, assmName);
                var result = _compileService.LoadType(schemaSrc, assmName);
                _map.Add(connectionString, result);
            }

            return _map[connectionString];
        }
    }
}
