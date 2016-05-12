namespace QueryEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class GuidHelper
    {
        public static string ToIdentifierWithPrefix(this Guid guid, string prefix)
        {
            return string.Format("{0}{1}", prefix, guid.ToString().Replace("-", ""));
        }
    }
}
