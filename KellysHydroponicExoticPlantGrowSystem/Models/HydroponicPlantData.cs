using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KellysHydroponicExoticPlantGrowSystem.Models
{
    public class HydroponicPlantData
    {
        public int MoistureValue { get; set; }
        public int LightingLevel { get; set; }
        public bool Lamp1On { get; set; }
        public bool Lamp2On { get; set; }
        public bool Lamp3On { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double BarometricPressure { get; set; }
        public double Altitude { get; set; }
        public double DewPoint { get; set; }
        public double HeatIndex { get; set; }
    }
}
