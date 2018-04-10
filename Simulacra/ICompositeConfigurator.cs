using System.Collections.Generic;

namespace Simulacra
{
    public interface ICompositeConfigurator<in T> : IConfigurator<T>
    {
        new IEnumerable<IConfigurator<T>> SubConfigurators { get; }
    }

    public interface ICompositeConfigurator<in T, out TSubConfigurator> : ICompositeConfigurator<T>
        where TSubConfigurator : IConfigurator<T>
    {
        new IEnumerable<TSubConfigurator> SubConfigurators { get; }
    }
}