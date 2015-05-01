using System.Collections.Generic;
using System.Linq;

namespace Diese.Modelization
{
    public class ConfiguratorDictionary<TKey, TValue, TValueData> : DataModelDictionary<TKey, TValue, TValueData>,
        IConfigurator<Dictionary<TKey, TValue>>
        where TValueData : IConfigurator<TValue>, new()
    {
        public void Configure(Dictionary<TKey, TValue> obj)
        {
            for (int i = 0; i < Count; i++)
                this.ElementAt(i).Value.Configure(obj.ElementAt(i).Value);
        }
    }

    public class ConfiguratorDictionary<TKey, TValue, TKeyData, TValueData> :
        DataModelDictionary<TKey, TValue, TKeyData, TValueData>, IConfigurator<Dictionary<TKey, TValue>>
        where TKeyData : IConfigurator<TKey>, new()
        where TValueData : IConfigurator<TValue>, new()
    {
        public void Configure(Dictionary<TKey, TValue> obj)
        {
            for (int i = 0; i < Count; i++)
            {
                KeyValuePair<TKeyData, TValueData> keyValuePair = this.ElementAt(i);
                keyValuePair.Key.Configure(obj.ElementAt(i).Key);
                keyValuePair.Value.Configure(obj.ElementAt(i).Value);
            }
        }
    }
}