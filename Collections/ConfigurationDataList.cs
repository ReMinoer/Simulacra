using System.Collections.Generic;

namespace Diese.Modelization.Collections
{
    public sealed class ConfigurationDataList<T, TData> : DataModelList<T, TData>, IConfigurationData<IList<T>>
        where TData : IConfigurationData<T>, new()
    {
        public void Configure(IList<T> obj)
        {
            for (int i = 0; i < Count; i++)
                this[i].Configure(obj[i]);
        }
    }
}