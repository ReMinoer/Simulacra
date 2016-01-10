using System;
using System.Collections.Generic;
using System.Linq;

namespace Diese.Modelization.Collections
{
    public sealed class CreationDataList<T> : DataModelList<T>, ICreationData<ICollection<T>>, IConfigurationData<ICollection<T>>
        where T : new()
    {
        public ICollection<T> Create()
        {
            return this.ToList();
        }

        public void Configure(ICollection<T> obj)
        {
            obj.Clear();

            foreach (T item in this)
                obj.Add(item);
        }
    }

    public sealed class CreationDataList<T, TData> : DataModelList<T, TData>, ICreationData<ICollection<T>>, IConfigurationData<ICollection<T>>
        where TData : ICreationData<T>, new()
    {
        public Action<TData> DataConfiguration { get; set; }

        public ICollection<T> Create()
        {
            return this.Select(CreateItem).ToList();
        }

        public void Configure(ICollection<T> obj)
        {
            obj.Clear();

            foreach (TData data in this)
                obj.Add(CreateItem(data));
        }

        private T CreateItem(TData data)
        {
            if (DataConfiguration != null)
                DataConfiguration(data);

            T item = data.Create();
            return item;
        }
    }
}