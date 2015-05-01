using System.Collections.Generic;

namespace Diese.Modelization
{
    public class ConfiguratorList<T, TData> : DataModelList<T, TData>, IConfigurator<List<T>>
        where TData : IConfigurator<T>, new()
    {
        public void Configure(List<T> obj)
        {
            for (int i = 0; i < Count; i++)
                this[i].Configure(obj[i]);
        }
    }
}