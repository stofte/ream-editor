using NUnit.Framework;
using LinqEditor.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Tests
{
    [TestFixture]
    public class StringHelperTests
    {
        [TestCase("1\n2\n\n4", 4)]
        [TestCase("1\n2\n\r\n4", 4)]
        [TestCase("1\n2\n\r4", 4)]
        public void Normalizes_String(string input, int lineCount)
        {
            var norm = input.NormalizeLines();
            var lines = norm.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.AreEqual(lineCount, lines.Count());
        }
    }
}
