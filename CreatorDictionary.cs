using System.Collections.Generic;

namespace Diese.Modelization
{
    public class CreatorDictionary<TKey, TValue> : DataModelDictionary<TKey, TValue>, ICreator<Dictionary<TKey, TValue>>
        where TValue : new()
    {
        public Dictionary<TKey, TValue> Create()
        {
            var list = new Dictionary<TKey, TValue>();
            foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
                list.Add(keyValuePair.Key, keyValuePair.Value);
            return list;
        }
    }

    public class CreatorDictionary<TKey, TValue, TValueData> : DataModelDictionary<TKey, TValue, TValueData>,
        ICreator<Dictionary<TKey, TValue>>
        where TValueData : ICreator<TValue>, new()
    {
        public Dictionary<TKey, TValue> Create()
        {
            var list = new Dictionary<TKey, TValue>();
            foreach (KeyValuePair<TKey, TValueData> keyValuePair in this)
                list.Add(keyValuePair.Key, keyValuePair.Value.Create());
            return list;
        }
    }

    public class CreatorDictionary<TKey, TValue, TKeyData, TValueData> :
        DataModelDictionary<TKey, TValue, TKeyData, TValueData>, ICreator<Dictionary<TKey, TValue>>
        where TKeyData : ICreator<TKey>, new()
        where TValueData : ICreator<TValue>, new()
    {
        public Dictionary<TKey, TValue> Create()
        {
            var list = new Dictionary<TKey, TValue>();
            foreach (KeyValuePair<TKeyData, TValueData> keyValuePair in this)
                list.Add(keyValuePair.Key.Create(), keyValuePair.Value.Create());
            return list;
        }
    }
}