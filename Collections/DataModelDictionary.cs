using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Diese.Modelization.Collections
{
    public class DataModelDictionary<TKey, TValue> : Dictionary<TKey, TValue>,
        IDataModel<Dictionary<TKey, TValue>>, IXmlSerializable
        where TValue : new()
    {
        public void From(Dictionary<TKey, TValue> obj)
        {
            Clear();
            foreach (KeyValuePair<TKey, TValue> keyValuePair in obj)
                Add(keyValuePair.Key, keyValuePair.Value);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("Item");

                reader.ReadStartElement("Key");
                var key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("Value");
                var value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in Keys)
            {
                writer.WriteStartElement("Item");

                writer.WriteStartElement("Key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("Value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
    }

    public class DataModelDictionary<TKey, TValue, TValueData> : Dictionary<TKey, TValueData>,
        IDataModel<Dictionary<TKey, TValue>>, IXmlSerializable
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

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValueData));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("Item");

                reader.ReadStartElement("Key");
                var key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("Value");
                var value = (TValueData)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValueData));

            foreach (TKey key in Keys)
            {
                writer.WriteStartElement("Item");

                writer.WriteStartElement("Key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("Value");
                TValueData value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
    }

    public class DataModelDictionary<TKey, TValue, TKeyData, TValueData> : Dictionary<TKeyData, TValueData>,
        IDataModel<Dictionary<TKey, TValue>>, IXmlSerializable
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

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var keySerializer = new XmlSerializer(typeof(TKeyData));
            var valueSerializer = new XmlSerializer(typeof(TValueData));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("Item");

                reader.ReadStartElement("Key");
                var key = (TKeyData)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("Value");
                var value = (TValueData)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            var keySerializer = new XmlSerializer(typeof(TKeyData));
            var valueSerializer = new XmlSerializer(typeof(TValueData));

            foreach (TKeyData key in Keys)
            {
                writer.WriteStartElement("Item");

                writer.WriteStartElement("Key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("Value");
                TValueData value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
    }
}