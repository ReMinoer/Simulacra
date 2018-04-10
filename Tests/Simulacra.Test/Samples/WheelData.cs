namespace Simulacra.Test.Samples
{
    public class WheelData : IDataModel<Wheel>, IConfigurator<Wheel>, ICreator<Wheel>
    {
        public double Wear { get; set; }

        public void From(Wheel obj)
        {
            Wear = obj.Wear;
        }

        public void Configure(Wheel obj)
        {
            obj.Wear = Wear;
        }

        public Wheel Create()
        {
            var obj = new Wheel();
            Configure(obj);
            return obj;
        }
    }
}