using System;

namespace LinqEditor.Core.Containers
{
    // http://www.superstarcoders.com/blogs/posts/executing-code-in-a-separate-application-domain-using-c-sharp.aspx
    public abstract class Isolated<T> : IDisposable where T : MarshalByRefObject, IContainer
    {
        private AppDomain _domain;
        private T _value;
        Guid _id;

        public Guid Id { get { return _id; } }

        public Isolated(Guid id)
        {
            _id = id;
            _domain = AppDomain.CreateDomain("Isolated:" + id,
               null, AppDomain.CurrentDomain.SetupInformation);

            Type type = typeof(T);

            _value = (T)_domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
        }

        public T Value
        {
            get
            {
                return _value;
            }
        }

        public void Dispose()
        {
            if (_domain != null)
            {
                AppDomain.Unload(_domain);

                _domain = null;
            }
        }
    }
}
