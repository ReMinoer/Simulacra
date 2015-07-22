using System.Collections.Generic;
using System.Linq;

namespace Diese.Modelization
{
    public class CreatorList<T> : DataModelList<T>, ICreator<List<T>>
        where T : new()
    {
        public List<T> Create()
        {
            return this.ToList();
        }
    }

    public class CreatorList<T, TData> : DataModelList<T, TData>, ICreator<List<T>>
        where TData : ICreator<T>, new()
    {
        public List<T> Create()
        {
            return this.Select(modelData => modelData.Create()).ToList();
        }
    }
}