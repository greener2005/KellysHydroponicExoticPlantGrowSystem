using KellysHydroponicExoticPlantGrowSystem.Interfaces;
using Microsoft.Practices.ServiceLocation;
using Restup.Webserver.Attributes;
using Restup.Webserver.Models.Schemas;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using KellysHydroponicExoticPlantGrowSystem.Enums;
using Newtonsoft.Json;

namespace KellysHydroponicExoticPlantGrowSystem.Controllers
{
    public class PlantSensorsController
    {
        private readonly IPlantMonitoringService _plantMonitoringService;

        public PlantSensorsController()
        {
            _plantMonitoringService = ServiceLocator.Current.GetInstance<IPlantMonitoringService>();
            InitPlantServices();
        }

        private void InitPlantServices()
        {
            foreach (HydroPonicLights hydro in Enum.GetValues(typeof(HydroPonicLights)))
                _plantMonitoringService.ToggleHydroponicLightOnOrOff(hydro);
        }

        [UriFormat("/GetPlantSensorData")]
        public GetResponse GetPlantSensorData() => new GetResponse(GetResponse.ResponseStatus.OK, JsonConvert.SerializeObject(_plantMonitoringService?.HydroponicPlantData));
    }
}