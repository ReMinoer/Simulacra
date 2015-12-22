using System.Collections.Generic;

namespace Diese.Modelization.Collections
{
    public class DataModelList<T> : List<T>, IDataModel<IList<T>>
        where T : new()
    {
        public void From(IList<T> obj)
        {
            Clear();
            foreach (T element in obj)
                Add(element);
        }
    }

    public class DataModelList<T, TData> : List<TData>, IDataModel<IList<T>>
        where TData : IDataModel<T>, new()
    {
        public void From(IList<T> obj)
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