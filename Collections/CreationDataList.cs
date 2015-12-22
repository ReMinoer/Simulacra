using System;
using System.Collections.Generic;
using System.Linq;

namespace Diese.Modelization.Collections
{
    public sealed class CreationDataList<T> : DataModelList<T>, ICreationData<IList<T>>
        where T : new()
    {
        public IList<T> Create()
        {
            return this.ToList();
        }
    }

    public sealed class CreationDataList<T, TData> : DataModelList<T, TData>, ICreationData<IList<T>>
        where TData : ICreationData<T>, new()
    {
        public Action<TData> DataConfiguration { get; set; }

        public IList<T> Create()
        {
            return this.Select(modelData =>
            {
                if (DataConfiguration != null)
                    DataConfiguration(modelData);

                T obj = modelData.Create();
                return obj;
            })
            .ToList();
        }
    }
}