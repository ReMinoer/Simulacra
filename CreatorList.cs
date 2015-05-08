using System.Collections.Generic;

namespace Diese.Modelization
{
    public class CreatorList<T> : DataModelList<T>, ICreator<List<T>>
        where T : new()
    {
        public List<T> Create()
        {
            var list = new List<T>();
            foreach (T data in this)
                list.Add(data);
            return list;
        }
    }

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