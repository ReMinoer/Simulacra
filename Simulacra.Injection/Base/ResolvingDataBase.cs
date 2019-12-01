﻿using System;
using System.Collections.Generic;
using Niddle;
using Simulacra.Binding;
using Simulacra.Binding.Collection;
using Simulacra.Binding.Property;
using Simulacra.Injection.Utils;

namespace Simulacra.Injection.Base
{
    public abstract class ResolvingDataBase<TData, T, TSubConfigurator> : NotifyPropertyChangedBase, IInstantiatingData<T>, ICreator<T>, ICompositeConfigurator<T, TSubConfigurator>, IDependencyResolverClient
        where TData : ResolvingDataBase<TData, T, TSubConfigurator>
        where TSubConfigurator : IConfigurator<T>
        where T : class
    {
        static public PropertyBindingCollection<TData, T> PropertyBindings { get; }
        static public CollectionBindingCollection<TData, T> CollectionBindings { get; }
        
        private readonly BindingManager<T> _bindingManager;
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
        public virtual IDependencyResolver DependencyResolver
        {
            protected get => _dependencyResolver;
            set => Set(ref _dependencyResolver, value);
        }

        IEnumerable<TSubConfigurator> ICompositeConfigurator<T, TSubConfigurator>.SubConfigurators => SubConfiguratorsBase;
        object IBindableData.BindedObject => BindedObject;

        static ResolvingDataBase()
        {
            PropertyBindings = new PropertyBindingCollection<TData, T>();
            CollectionBindings = new CollectionBindingCollection<TData, T>();
        }

        protected ResolvingDataBase()
        {
            var owner = (TData)this;

            _bindingManager = new BindingManager<T>
            {
                Modules =
                {
                    new PropertyBindingModule<TData, T>(owner, PropertyBindings),
                    new CollectionBindingModule<TData, T>(owner, CollectionBindings)
                }
            };
        }

        public virtual void Instantiate()
        {
            if (BindedObject != null)
                throw new InvalidOperationException();

            BindedObject = Create();
            _bindingManager.BindView(BindedObject);

            IsInstantiated = true;
        }

        public virtual void Dispose()
        {
            _bindingManager.UnbindView();

            DisposeBindedObject();

            BindedObject = default(T);
            IsInstantiated = false;
        }

        protected virtual void DisposeBindedObject() => (BindedObject as IDisposable)?.Dispose();

        private T Create()
        {
            T obj = New();
            Configure(obj);
            return obj;
        }

        protected virtual T New() => DependencyResolver.Resolve<T>();

        protected void Configure(T obj)
        {
            if (obj == null)
                return;

            _bindingManager.InitializeView(obj);

            foreach (TSubConfigurator subConfigurator in SubConfiguratorsBase)
                subConfigurator.Configure(obj);
        }

        T ICreator<T>.Create() => Create();
        void IConfigurator<T>.Configure(T obj) => Configure(obj);
    }
}