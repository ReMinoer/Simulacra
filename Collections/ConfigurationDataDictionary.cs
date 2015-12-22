using System.Collections.Generic;
using System.Linq;

namespace Diese.Modelization.Collections
{
    public sealed class ConfigurationDataDictionary<TKey, TValue, TValueData> : DataModelDictionary<TKey, TValue, TValueData>,
        IConfigurationData<Dictionary<TKey, TValue>>
        where TValueData : IConfigurationData<TValue>, new()
    {
        public void Configure(Dictionary<TKey, TValue> obj)
        {
            for (int i = 0; i < Count; i++)
                this.ElementAt(i).Value.Configure(obj.ElementAt(i).Value);
        }
    }

    public sealed class ConfigurationDataDictionary<TKey, TValue, TKeyData, TValueData> :
        DataModelDictionary<TKey, TValue, TKeyData, TValueData>, IConfigurationData<Dictionary<TKey, TValue>>
        where TKeyData : IConfigurationData<TKey>, new()
        where TValueData : IConfigurationData<TValue>, new()
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