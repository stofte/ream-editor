using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Utility.Helpers
{
    public static class SerializationHelper
    {
        public static string Hash(object o)
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            var hasher = SHA256Cng.Create(); // not sure if instance can be shared, so recreating instance each time
            hasher.Initialize();

            formatter.Serialize(stream, o);
            stream.Position = 0;
            var byteKey = hasher.ComputeHash(stream);

            var sb = new StringBuilder();
            foreach (var b in byteKey)
            {
                sb.Append(b.ToString("x2"));
            }
            var str = sb.ToString();

            return str;
        }
    }

         
}
