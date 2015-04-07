using LinqEditor.Core;
using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.Helpers;
using LinqEditor.Core.Models.Editor;
using LinqEditor.Core.Models.Database;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LinqEditor.Core.Settings;

namespace LinqEditor.Core.Templates.Tests
{
    [TestFixture]
    public class TemplateServiceTests
    {
        DatabaseSchema _schemaModel;
        Guid _schemaId = Guid.NewGuid();
        string _schemaPath;

        [TestFixtureSetUp]
        public void Initialize()
        {
            _schemaPath = PathUtility.TempPath + _schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix) + ".dll";

            _schemaModel = new DatabaseSchema
            {
                Connection = new Connection(),
                Tables = new List<TableSchema>
                {
                    new TableSchema 
                    { 
                        Catalog = "catalog",
                        Schema= "schema",
                        Name = "table",
                        Columns = new List<ColumnSchema> 
                        {
                            new ColumnSchema { Index = 0, Name = "PK_ID", TypeName = "int" },
                            new ColumnSchema { Index = 1, Name = "Description", TypeName = "string" },
                            new ColumnSchema { Index = 2, Name = "Name", TypeName = "string" }
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
            var result = CSharpCompiler.CompileToBytes(source, _schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix));
            Assert.AreEqual(0, result.Errors.Count());
        }

        [Test]
        public void Generates_Valid_Query_Database_Source_Code()
        {
            var templateService = new TemplateService();

            var schemaSource = templateService.GenerateSchema(_schemaId, _schemaModel);
            var schemaResult = CSharpCompiler.CompileToFile(schemaSource, _schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix), PathUtility.TempPath);

            var queryId = Guid.NewGuid();
            var source = templateService.GenerateQuery(queryId, "table.Count();", _schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix));

            var result = CSharpCompiler.CompileToBytes(source, _schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix), _schemaPath);
            Assert.AreEqual(0, result.Errors.Count());
        }

        [Test]
        public void Generates_Valid_Code_Statements_Source_Code()
        {
            var templateService = new TemplateService();
            var codeId = Guid.NewGuid();
            var source = templateService.GenerateCodeStatements(codeId, "int x = 10;");
            var result = CSharpCompiler.CompileToBytes(source, codeId.ToIdentifierWithPrefix(SchemaConstants.CodePrefix));
            Assert.AreEqual(0, result.Errors.Count());
        }
    }
}
