using System.Collections.Generic;

namespace Diese.Modelization.Collections
{
    public class DataModelList<T> : List<T>, IDataModel<ICollection<T>>
    {
        public void From(ICollection<T> obj)
        {
            Clear();
            foreach (T element in obj)
                Add(element);
        }
    }

    public class DataModelList<T, TData> : List<TData>, IDataModel<ICollection<T>>
        where TData : IDataModel<T>, new()
    {
        public void From(ICollection<T> obj)
        {
            Clear();
            foreach (T element in obj)
            {
                var dataModel = new TData();
                dataModel.From(element);
                Add(dataModel);
            }
        }
    }
}