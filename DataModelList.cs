using System.Collections.Generic;

namespace Diese.Modelization
{
    public class DataModelList<T, TData> : List<TData>, IDataModel<List<T>>
        where TData : IDataModel<T>, new()
    {
        public void From(List<T> obj)
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