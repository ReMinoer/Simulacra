using System;
using System.Collections.Generic;
using System.Linq;

namespace Diese.Modelization.Collections
{
    public sealed class CreationDataDictionary<TKey, TValue> : DataModelDictionary<TKey, TValue>, ICreationData<Dictionary<TKey, TValue>>
        where TValue : new()
    {
        public Dictionary<TKey, TValue> Create()
        {
            return this.ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }

    public sealed class CreationDataDictionary<TKey, TValue, TValueData> : DataModelDictionary<TKey, TValue, TValueData>,
        ICreationData<Dictionary<TKey, TValue>>
        where TValueData : ICreationData<TValue>, new()
    {
        public Action<TValueData> ValueDataConfiguration { get; set; }

        public Dictionary<TKey, TValue> Create()
        {
            return this.ToDictionary(pair => pair.Key, pair =>
            {
                TValueData modelData = pair.Value;

                if (ValueDataConfiguration != null)
                    ValueDataConfiguration(modelData);

                TValue obj = modelData.Create();
                return obj;
            });
        }
    }

    public sealed class CreationDataDictionary<TKey, TValue, TKeyData, TValueData> :
        DataModelDictionary<TKey, TValue, TKeyData, TValueData>, ICreationData<Dictionary<TKey, TValue>>
        where TKeyData : ICreationData<TKey>, new()
        where TValueData : ICreationData<TValue>, new()
    {
        public Action<TKeyData> KeyDataConfiguration { get; set; }
        public Action<TValueData> ValueDataConfiguration { get; set; }

        public Dictionary<TKey, TValue> Create()
        {
            return this.ToDictionary(pair =>
            {
                TKeyData modelData = pair.Key;

                if (KeyDataConfiguration != null)
                    KeyDataConfiguration(modelData);

                TKey obj = modelData.Create();
                return obj;
            }, pair =>
            {
                TValueData modelData = pair.Value;

                if (ValueDataConfiguration != null)
                    ValueDataConfiguration(modelData);

                TValue obj = modelData.Create();
                return obj;
            });
        }
    }
}