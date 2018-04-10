using System.Collections.Generic;

namespace Simulacra.Collections
{
    public class DataModelDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDataModel<Dictionary<TKey, TValue>>
    {
        public void From(Dictionary<TKey, TValue> obj)
        {
            Clear();
            foreach (KeyValuePair<TKey, TValue> keyValuePair in obj)
                Add(keyValuePair.Key, keyValuePair.Value);
        }
    }

    public class DataModelDictionary<TKey, TValue, TValueData> : Dictionary<TKey, TValueData>, IDataModel<Dictionary<TKey, TValue>>
        where TValueData : IDataModel<TValue>, new()
    {
        public void From(Dictionary<TKey, TValue> obj)
        {
            Clear();
            foreach (KeyValuePair<TKey, TValue> keyValuePair in obj)
            {
                var dataModel = new TValueData();
                dataModel.From(keyValuePair.Value);
                Add(keyValuePair.Key, dataModel);
            }
        }
    }

    public class DataModelDictionary<TKey, TValue, TKeyData, TValueData> : Dictionary<TKeyData, TValueData>, IDataModel<Dictionary<TKey, TValue>>
        where TKeyData : IDataModel<TKey>, new()
        where TValueData : IDataModel<TValue>, new()
    {
        public void From(Dictionary<TKey, TValue> obj)
        {
            Clear();
            foreach (KeyValuePair<TKey, TValue> keyValuePair in obj)
            {
                var keyDataModel = new TKeyData();
                var valueDataModel = new TValueData();

                keyDataModel.From(keyValuePair.Key);
                valueDataModel.From(keyValuePair.Value);

                Add(keyDataModel, valueDataModel);
            }
        }
    }
}