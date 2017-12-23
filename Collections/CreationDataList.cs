using System;
using System.Collections.Generic;
using System.Linq;

namespace Diese.Modelization.Collections
{
    public sealed class CreationDataList<T> : DataModelList<T>, ICreator<ICollection<T>>, IConfigurator<ICollection<T>>
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

    public sealed class CreationDataList<T, TData> : DataModelList<T, TData>, ICreator<ICollection<T>>, IConfigurator<ICollection<T>>
        where TData : IDataModel<T>, ICreator<T>, new()
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
            DataConfiguration?.Invoke(data);

            T item = data.Create();
            return item;
        }
    }
}