using LinqEditor.Core.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Tests
{
    [TestFixture]
    public class SerializationHelperTests
    {
        [Serializable]
        class Foo
        {
            public int Val;
        }

        class NotSerializable
        {
            public int Val;
        }

        [Test]
        public void Can_Compute_Hash_For_Serializable_Object()
        {
            var o = new object();
            var hash = SerializationHelper.Hash(o);

            Assert.IsNotNullOrEmpty(hash);
        }

        [Test]
        public void Object_Must_Be_Non_Null()
        {
            Assert.Throws<ArgumentNullException>(() => SerializationHelper.Hash(null));
        }

        [Test]
        public void Object_Must_Be_Serializable()
        {
            var o = new NotSerializable { Val = 1 };
            Assert.Throws<SerializationException>(() => SerializationHelper.Hash(o));
        }

        [Test]
        public void Different_Object_Computes_Different_Hashes()
        {
            var o1 = new Foo { Val = 1 };
            var o2 = new Foo { Val = 2 };
            var hash1 = SerializationHelper.Hash(o1);
            var hash2 = SerializationHelper.Hash(o2);

            Assert.AreNotEqual(hash1, hash2);
        }
    }
}
