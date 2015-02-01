using System;

namespace LinqEditor.Core
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
