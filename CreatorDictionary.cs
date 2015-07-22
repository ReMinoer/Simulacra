using System.Collections.Generic;
using System.Linq;

namespace Diese.Modelization
{
    public class CreatorDictionary<TKey, TValue> : DataModelDictionary<TKey, TValue>, ICreator<Dictionary<TKey, TValue>>
        where TValue : new()
    {
        public Dictionary<TKey, TValue> Create()
        {
            return this.ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }

    public class CreatorDictionary<TKey, TValue, TValueData> : DataModelDictionary<TKey, TValue, TValueData>,
        ICreator<Dictionary<TKey, TValue>>
        where TValueData : ICreator<TValue>, new()
    {
        public Dictionary<TKey, TValue> Create()
        {
            return this.ToDictionary(pair => pair.Key, pair => pair.Value.Create());
        }
    }

    public class CreatorDictionary<TKey, TValue, TKeyData, TValueData> :
        DataModelDictionary<TKey, TValue, TKeyData, TValueData>, ICreator<Dictionary<TKey, TValue>>
        where TKeyData : ICreator<TKey>, new()
        where TValueData : ICreator<TValue>, new()
    {
        public Dictionary<TKey, TValue> Create()
        {
            return this.ToDictionary(pair => pair.Key.Create(), pair => pair.Value.Create());
        }
    }
}