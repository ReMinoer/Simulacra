using System.Collections.Generic;

namespace Diese.Modelization.Collections
{
    public sealed class ConfigurationDataList<T, TData> : DataModelList<T, TData>, IConfigurator<ICollection<T>>
        where TData : IDataModel<T>, IConfigurator<T>, new()
    {
        public void Configure(ICollection<T> obj)
        {
            int i = 0;
            foreach (T item in obj)
            {
                this[i].Configure(item);
                i++;
            }
        }
    }
}