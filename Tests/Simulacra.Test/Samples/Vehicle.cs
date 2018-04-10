using System.Collections.Generic;

namespace Simulacra.Test.Samples
{
    public class Vehicle
    {
        public int SpeedMax { get; set; }
        public ICollection<Wheel> Wheels { get; set; }
        public Dictionary<string, Passenger> Passengers { get; set; }
        public int CurrentSpeed { get; set; }

        public Vehicle()
        {
            Passengers = new Dictionary<string, Passenger>();
            Wheels = new List<Wheel>();
        }
    }
}