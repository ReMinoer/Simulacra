using System.Collections.Generic;

namespace Diese.Modelization
{
    public class DataModelList<T, TData> : List<TData>, IDataModel<List<T>>
        where TData : IDataModel<T>, new()
    {
        public void From(List<T> obj)
        {
            Clear();
            foreach (T passenger in obj)
            {
                var dataModel = new TData();
                dataModel.From(passenger);
                Add(dataModel);
            }
        }
    }
}