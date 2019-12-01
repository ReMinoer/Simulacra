using Niddle;

namespace Simulacra.Injection
{
    public interface IDependencyResolverClient
    {
        IDependencyResolver DependencyResolver { set; }
    }
}