using LinqEditor.Common.Tests;
using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.Schema.Models;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using LinqEditor.Core.Schema.Helpers;
using System.IO;

namespace LinqEditor.Core.Templates.Tests
{
    [TestFixture]
    public class TemplateServiceTests
    {
        DatabaseSchema _schemaModel;
        Guid _schemaId = Guid.NewGuid();
        string _schemaPath;
        ICSharpCompiler _compiler;

        [TestFixtureSetUp]
        public void Initialize()
        {
            _schemaPath = Path.GetTempPath() + _schemaId.ToIdentifierWithPrefix("s") + ".dll";
            _compiler = new CSharpCompiler();

            _schemaModel = new DatabaseSchema
            {
                Tables = new List<TableSchema>
                {
                    new TableSchema 
                    { 
                        Catalog = "catalog",
                        Schema= "schema",
                        Name = "table",
                        Columns = new List<ColumnSchema> 
                        {
                            new ColumnSchema { Index = 0, Name = "PK_ID", Type = "int" },
                            new ColumnSchema { Index = 1, Name = "Description", Type = "string" },
                            new ColumnSchema { Index = 2, Name = "Name", Type = "string" }
                        }
                    }
                }
            };
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            if (File.Exists(_schemaPath))
            {
                File.Delete(_schemaPath);
            }
        }

        [Test]
        public void Generates_Valid_Schema_Database_Source_Code()
        {
            var templateService = new TemplateService();
            var source = templateService.GenerateSchema(_schemaId, _schemaModel);
            var result = _compiler.Compile(source, _schemaId.ToIdentifierWithPrefix("s"));
            Assert.AreEqual(0, result.Errors.Count());
        }

        [Test]
        public void Generates_Valid_Query_Database_Source_Code()
        {
            var templateService = new TemplateService();

            var schemaSource = templateService.GenerateSchema(_schemaId, _schemaModel);
            var schemaResult = _compiler.Compile(schemaSource, _schemaId.ToIdentifierWithPrefix("s"), Path.GetTempPath());

            var queryId = Guid.NewGuid();
            var source = templateService.GenerateQuery(queryId, "table.Count();", _schemaId.ToIdentifierWithPrefix("s"));

            var result = _compiler.Compile(source, _schemaId.ToIdentifierWithPrefix("s"), null, _schemaPath);
            Assert.AreEqual(0, result.Errors.Count());
        }

        [Test]
        public void Generates_Valid_Code_Statements_Source_Code()
        {
            var templateService = new TemplateService();
            var codeId = Guid.NewGuid();
            var source = templateService.GenerateCodeStatements(codeId, "int x = 10;");
            var result = _compiler.Compile(source, codeId.ToIdentifierWithPrefix("c"));
            Assert.AreEqual(0, result.Errors.Count());
        }
    }
}
