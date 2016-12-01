using KellysHydroponicExoticPlantGrowSystem.Interfaces;
using Microsoft.Practices.ServiceLocation;
using Restup.Webserver.Attributes;
using Restup.Webserver.Models.Schemas;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KellysHydroponicExoticPlantGrowSystem.Controllers
{
    public class PlantSensorsController
    {
        private readonly IPlantMonitoringService _plantMonitoringService;

        public PlantSensorsController()
        {
            _plantMonitoringService = ServiceLocator.Current.GetInstance<IPlantMonitoringService>();
            InitPlantServices().ContinueWith(t => Debug.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task InitPlantServices()
        {
            Debug.WriteLine("test");
        }

        [UriFormat("/GetPlantSensorData")]
        public async Task<GetResponse> GetPlantSensorData()
        {
            return null;
        }
    }
}