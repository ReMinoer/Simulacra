using System.Collections.Generic;

namespace Simulacra
{
    public interface ICompositeConfigurator<in T, out TSubConfigurator> : IConfigurator<T>
        where TSubConfigurator : IConfigurator<T>
    {
        IEnumerable<TSubConfigurator> SubConfigurators { get; }
    }
}