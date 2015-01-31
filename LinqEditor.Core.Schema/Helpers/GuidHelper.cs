using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Schema.Helpers
{
    public static class GuidHelper
    {
        public static string ToIdentifierWithPrefix(this Guid guid, string prefix)
        {
            return string.Format("{0}{1}", prefix, guid.ToString().Replace("-", ""));
        }
    }
}
