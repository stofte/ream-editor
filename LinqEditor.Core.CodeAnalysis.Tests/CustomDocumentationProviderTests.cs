using LinqEditor.Core.CodeAnalysis.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    [TestFixture]
    public class CustomDocumentationProviderTests
    {
        class TestCustomProvider : CustomDocumentationProvider
        {
            public TestCustomProvider(Assembly assembly) : base(assembly) { }

            public string GetDoc(string documentationMemberID)
            {
                 var culture = System.Globalization.CultureInfo.DefaultThreadCurrentCulture;
                 return GetDocumentationForSymbol(documentationMemberID, culture, CancellationToken.None);
            }
        }

        [Test]
        public void Providers_With_Same_Assembly_Compares_As_Equal()
        {
            var provider1 = new CustomDocumentationProvider(typeof(int).Assembly);
            var provider2 = new CustomDocumentationProvider(typeof(int).Assembly);

            Assert.IsTrue(provider1.Equals(provider2));
        }

        [Test]
        public void Providers_With_Different_Assemblies_Compares_As_Unequal()
        {
            var provider1 = new CustomDocumentationProvider(typeof(int).Assembly);
            var provider2 = new CustomDocumentationProvider(typeof(TestAttribute).Assembly);

            Assert.IsFalse(provider1.Equals(provider2));
        }

        [Test]
        public void Returns_Documentation_For_Symbol()
        {
            var provider = new TestCustomProvider(typeof(int).Assembly);
            
            var expectedDoc = XElement.Parse(@"
<member name=""T:System.Int32"">
  <summary>Represents a 32-bit signed integer.</summary>
  <filterpriority>1</filterpriority>
</member>
");

            var docId = "T:System.Int32";
            var docResult = XElement.Parse(provider.GetDoc(docId));
            Assert.AreEqual(expectedDoc.ToString(), docResult.ToString());
        }
    }
}
