using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Simulacra.Binding;
using Simulacra.Binding.Base;
using Simulacra.IO.Utils;
using Simulacra.IO.Watching;

namespace Simulacra.IO.Binding
{
    static public class PathBindingModule
    {
        static public SynchronizationContext DefaultSynchronizationContext { get; set; }

        static private PathWatcher _watcher;
        static public PathWatcher Watcher => _watcher ?? (_watcher = new PathWatcher());
    }

    public class PathBindingModule<TModel, TView> : SubscriptionBindingModuleBase<TModel, TView, IOneWaySubscriptionBinding<TModel, TView, string>, string, FileChangedEventHandler>
        where TModel : class, INotifyPropertyChanged
        where TView : class
    {
        private readonly SynchronizationContext _synchronizationContext;
        public SynchronizationContext SynchronizationContext => _synchronizationContext ?? PathBindingModule.DefaultSynchronizationContext;

        private Subject<(IOneWaySubscriptionBinding<TModel, TView, string> binding, Action handlerAction)> _bindingHandlersSubject;
        private IDisposable _watcherThrottle;

        public PathBindingModule(TModel model, Dictionary<string, IOneWaySubscriptionBinding<TModel, TView, string>> bindings, SynchronizationContext synchronizationContext = null)
            : base(model, bindings)
        {
            _synchronizationContext = synchronizationContext;
        }

        public override void BindView(TView view)
        {
            base.BindView(view);

            // Throttle FileChanged handlers in case a software made multiple quick access.
            _bindingHandlersSubject = new Subject<(IOneWaySubscriptionBinding<TModel, TView, string>, Action)>();
            _watcherThrottle = _bindingHandlersSubject
                .GroupBy(x => x.binding, x => x.handlerAction)
                .Select(x => x.Throttle(TimeSpan.FromMilliseconds(10)))
                .SelectMany(x => x)
                .Subscribe(handlerAction => handlerAction());
        }

        public override void UnbindView()
        {
            _watcherThrottle.Dispose();
            _bindingHandlersSubject.Dispose();

            base.UnbindView();
        }

        protected override void Subscribe(string path, FileChangedEventHandler handler)
        {
            if (PathUtils.IsValidAbsolutePath(path))
                PathBindingModule.Watcher.WatchFile(path, handler);
        }

        protected override void Unsubscribe(string path, FileChangedEventHandler handler)
        {
            if (PathUtils.IsValidAbsolutePath(path))
                PathBindingModule.Watcher.Unwatch(handler);
        }

        protected override FileChangedEventHandler GetHandler(IOneWaySubscriptionBinding<TModel, TView, string> binding)
        {
            SynchronizationContext synchronizationContext = SynchronizationContext;

            void Handler()
            {
                if (synchronizationContext != null)
                    synchronizationContext.Send(x => binding.SetView(Model, View), null);
                else
                    binding.SetView(Model, View);
            }

            return (sender, e) => _bindingHandlersSubject.OnNext((binding, Handler));
        }
    }
}