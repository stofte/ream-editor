using LinqEditor.Core.CodeAnalysis.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    [TestFixture]
    public class DocumentationServiceTests
    {
        IDocumentationService _service;

        [TestFixtureSetUp]
        public void Initialize()
        {
            _service = new DocumentationService();
        }

        [TestCase("T:System.Int32", @"
<member name=""T:System.Int32"">
  <summary>Represents a 32-bit signed integer.</summary>
  <filterpriority>1</filterpriority>
</member>
")]
        [TestCase("T:System.Collections.Generic.HashSet`1", @"
    <member name=""T:System.Collections.Generic.HashSet`1"">
      <summary>Represents a set of values.</summary>
      <typeparam name=""T"">The type of elements in the hash set.</typeparam>
    </member>
")]
        public void Can_Return_Documentation_For_Members(string memberName, string expectedDocumentation)
        {
            var doc = _service.GetDocumentation(memberName);

            Assert.AreEqual(doc.ToString(), XElement.Parse(expectedDocumentation).ToString());

        }
    }
}
