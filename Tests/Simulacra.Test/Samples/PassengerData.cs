namespace Simulacra.Test.Samples
{
    public class PassengerData : IDataModel<Passenger>, IConfigurator<Passenger>, ICreator<Passenger>
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public void From(Passenger obj)
        {
            Name = obj.Name;
            Age = obj.Age;
        }

        public void Configure(Passenger obj)
        {
            obj.Name = Name;
            obj.Age = Age;
        }

        public Passenger Create()
        {
            var obj = new Passenger();
            Configure(obj);
            return obj;
        }
    }
}