using System.Collections.Generic;

namespace Diese.Modelization
{
    public class CreatorList<T, TData> : DataModelList<T, TData>, ICreator<List<T>>
        where TData : ICreator<T>, new()
    {
        public List<T> Create()
        {
            var list = new List<T>();
            foreach (TData modelData in this)
                list.Add(modelData.Create());
            return list;
        }
    }
}