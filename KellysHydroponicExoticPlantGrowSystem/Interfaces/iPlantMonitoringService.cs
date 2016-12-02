using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KellysHydroponicExoticPlantGrowSystem.Enums;
using KellysHydroponicExoticPlantGrowSystem.Models;

namespace KellysHydroponicExoticPlantGrowSystem.Interfaces
{
    public interface IPlantMonitoringService
    {
        int ReadMoistureSensor(MoistureSensor sensor);
        void ToggleHydroponicLightOnOrOff(HydroPonicLights lights);
        HydroponicPlantData HydroponicPlantData { get; }
    }
}
