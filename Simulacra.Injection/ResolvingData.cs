using System.Collections.Generic;
using Simulacra.Injection.Base;

namespace Simulacra.Injection
{
    public class ResolvingData<TData, T> : ResolvingDataBase<TData, T, IConfigurator<T>>
        where TData : ResolvingDataBase<TData, T, IConfigurator<T>>
        where T : class
    {
        public List<IConfigurator<T>> SubConfigurators { get; }
        protected override IEnumerable<IConfigurator<T>> SubConfiguratorsBase => SubConfigurators;

        public ResolvingData()
        {
            SubConfigurators = new List<IConfigurator<T>>();
        }
    }
}