using Castle.Facilities.TypedFactory;
using LinqEditor.Core.Models.Editor;
using System;
using System.Reflection;

namespace LinqEditor.Core.Containers
{
    public interface IContainerFactory<T>
    {
        T Create();
        void Release(T container);
    }

    public class ContainerSelector : DefaultTypedFactoryComponentSelector
    {
        protected override Type GetComponentType(MethodInfo method, object[] arguments)
        {
            var container = arguments[0];
            var handlerType = typeof(Isolated<>).MakeGenericType(container.GetType());
            return handlerType;
        }
    }

    // http://www.superstarcoders.com/blogs/posts/executing-code-in-a-separate-application-domain-using-c-sharp.aspx
    public class Isolated<T> : IDisposable where T : MarshalByRefObject, IContainer
    {
        private AppDomain _domain;
        private T _value;

        public Isolated(Guid id)
        {
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

    public class IsolatedDatabaseContainer : Isolated<DatabaseContainer>, IIsolatedDatabaseContainer 
    {
        public IsolatedDatabaseContainer(Guid id) : base(id) {}
    }

    public class IsolatedCodeContainer : Isolated<CodeContainer>, IIsolatedCodeContainer 
    {
        public IsolatedCodeContainer(Guid id) : base(id) { }
    }

    public interface IIsolatedDatabaseContainer : IDisposable
    {
        DatabaseContainer Value { get; }
    }

    public interface IIsolatedCodeContainer : IDisposable
    {
        CodeContainer Value { get; }
    }

    public interface IIsolatedDatabaseContainerFactory
    {
        IIsolatedDatabaseContainer Create(Guid id);
        void Release(IIsolatedDatabaseContainer instance);
    }

    public interface IIsolatedCodeContainerFactory
    {
        IIsolatedCodeContainer Create(Guid id);
        void Release(IIsolatedCodeContainer instance);
    }
}
