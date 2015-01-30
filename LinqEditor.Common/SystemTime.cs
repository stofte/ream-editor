using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Common
{
    /// <summary>
    /// Fascilitates testing of DateTime usage. 
    /// </summary>
    public static class SystemTime
    {
        private static Func<DateTime> _provider = () => DateTime.Now;

        public static DateTime Now
        {
            get { return _provider(); }
        }

        public static Func<DateTime> Provider
        {
            get
            {
                return _provider;
            }
            set
            {
                _provider = value;
            }
        }
    }
}
