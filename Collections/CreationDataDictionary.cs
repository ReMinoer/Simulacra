using System;
using System.Collections.Generic;
using System.Linq;

namespace Diese.Modelization.Collections
{
    public sealed class CreationDataDictionary<TKey, TValue> : DataModelDictionary<TKey, TValue>, ICreator<Dictionary<TKey, TValue>>, IConfigurator<Dictionary<TKey, TValue>>
    {
        public Dictionary<TKey, TValue> Create()
        {
            return this.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public void Configure(Dictionary<TKey, TValue> obj)
        {
            obj.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in this)
                obj.Add(pair.Key, pair.Value);
        }
    }

    public sealed class CreationDataDictionary<TKey, TValue, TValueData> : DataModelDictionary<TKey, TValue, TValueData>, ICreator<Dictionary<TKey, TValue>>, IConfigurator<Dictionary<TKey, TValue>>
        where TValueData : IDataModel<TValue>, ICreator<TValue>, new()
    {
        public Action<TValueData> ValueDataConfiguration { get; set; }

        public Dictionary<TKey, TValue> Create()
        {
            return this.ToDictionary(pair => pair.Key, CreateValue);
        }

        public void Configure(Dictionary<TKey, TValue> obj)
        {
            obj.Clear();

            foreach (KeyValuePair<TKey, TValueData> pair in this)
                obj.Add(pair.Key, CreateValue(pair));
        }

        private TValue CreateValue(KeyValuePair<TKey, TValueData> pair)
        {
            TValueData data = pair.Value;

            ValueDataConfiguration?.Invoke(data);

            TValue item = data.Create();
            return item;
        }
    }

    public sealed class CreationDataDictionary<TKey, TValue, TKeyData, TValueData> : DataModelDictionary<TKey, TValue, TKeyData, TValueData>, ICreator<Dictionary<TKey, TValue>>, IConfigurator<Dictionary<TKey, TValue>>
        where TKeyData : IDataModel<TKey>, ICreator<TKey>, new()
        where TValueData : IDataModel<TValue>, ICreator<TValue>, new()
    {
        public Action<TKeyData> KeyDataConfiguration { get; set; }
        public Action<TValueData> ValueDataConfiguration { get; set; }

        public Dictionary<TKey, TValue> Create()
        {
            return this.ToDictionary(CreateKey, CreateValue);
        }

        public void Configure(Dictionary<TKey, TValue> obj)
        {
            obj.Clear();

            foreach (KeyValuePair<TKeyData, TValueData> pair in this)
                obj.Add(CreateKey(pair), CreateValue(pair));
        }

        private TKey CreateKey(KeyValuePair<TKeyData, TValueData> pair)
        {
            TKeyData data = pair.Key;

            KeyDataConfiguration?.Invoke(data);

            TKey item = data.Create();
            return item;
        }

        private TValue CreateValue(KeyValuePair<TKeyData, TValueData> pair)
        {
            TValueData data = pair.Value;

            ValueDataConfiguration?.Invoke(data);

            TValue item = data.Create();
            return item;
        }
    }
}