using System;
using System.Collections.Generic;
using Niddle;
using Simulacra.Binding;
using Simulacra.Injection.Utils;

namespace Simulacra.Injection.Base
{
    public abstract class ResolvingDataBase<TData, T, TSubConfigurator> : NotifyPropertyChangedBase, IInstantiatingData<T>, ICreator<T>, ICompositeConfigurator<T, TSubConfigurator>, IDependencyResolverClient
        where TData : ResolvingDataBase<TData, T, TSubConfigurator>
        where TSubConfigurator : IConfigurator<T>
        where T : class
    {
        protected readonly TData Owner;
        protected readonly BindingManager<T> BindingManager;
        protected abstract IEnumerable<TSubConfigurator> SubConfiguratorsBase { get; }

        private T _bindedObject;
        public T BindedObject
        {
            get => _bindedObject;
            private set => Set(ref _bindedObject, value);
        }

        private bool _isInstantiated;
        public bool IsInstantiated
        {
            get => _isInstantiated;
            private set => Set(ref _isInstantiated, value);
        }

        private IDependencyResolver _dependencyResolver;
        protected virtual IDependencyResolver DependencyResolver
        {
            get => _dependencyResolver;
            set => Set(ref _dependencyResolver, value);
        }
        IDependencyResolver IDependencyResolverClient.DependencyResolver
        {
            get => DependencyResolver;
            set => DependencyResolver = value;
        }

        IEnumerable<TSubConfigurator> ICompositeConfigurator<T, TSubConfigurator>.SubConfigurators => SubConfiguratorsBase;
        object IBindableData.BindedObject => BindedObject;

        protected ResolvingDataBase()
        {
            Owner = (TData)this;
            BindingManager = new BindingManager<T>();
        }

        public virtual void Instantiate()
        {
            if (BindedObject != null)
            {
                BindingManager.UnbindView();
                DisposeBindedObject();
            }

            BindedObject = Create();
            BindingManager.BindView(BindedObject);

            IsInstantiated = true;
        }

        public virtual void Dispose()
        {
            BindingManager.UnbindView();
            DisposeBindedObject();
        }

        protected virtual void DisposeBindedObject() => (BindedObject as IDisposable)?.Dispose();

        public T Create()
        {
            T obj = New();
            Configure(obj);
            return obj;
        }

        protected virtual T New() => DependencyResolver.Resolve<T>();

        public void Configure(T obj)
        {
            if (obj == null)
                return;

            BindingManager.InitializeView(obj);

            foreach (TSubConfigurator subConfigurator in SubConfiguratorsBase)
                subConfigurator.Configure(obj);
        }
    }
}