using Simulacra.Collections;

namespace Simulacra.Test.Samples
{
    public class VehicleData : IDataModel<Vehicle>, IConfigurator<Vehicle>, ICreator<Vehicle>
    {
        public int SpeedMax { get; set; }
        public CreationDataDictionary<string, Passenger, PassengerData> Passengers { get; set; }
        public CreationDataList<Wheel, WheelData> Wheels { get; set; }

        public VehicleData()
        {
            Passengers = new CreationDataDictionary<string, Passenger, PassengerData>();
            Wheels = new CreationDataList<Wheel, WheelData>();
        }

        public void From(Vehicle obj)
        {
            SpeedMax = obj.SpeedMax;
            Passengers.From(obj.Passengers);
            Wheels.From(obj.Wheels);
        }

        public void Configure(Vehicle obj)
        {
            obj.SpeedMax = SpeedMax;
            obj.Passengers = Passengers.Create();
            obj.Wheels = Wheels.Create();
        }

        public Vehicle Create()
        {
            var obj = new Vehicle();
            Configure(obj);
            return obj;
        }
    }
}