using KellysHydroponicExoticPlantGrowSystem.Models;

namespace KellysHydroponicExoticPlantGrowSystem.Services
{
    using Enums;
    using Interfaces;
    using Sensors;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Windows.Devices.Gpio;

    public class PlantMonitoringService : IPlantMonitoringService
    {
        private MCP3008 _mcp3008;
        private GpioController _gpioController;
        private List<GpioPin> _relaySensorLightPins;
        private BME280Sensor _bme280Sensor;

        public PlantMonitoringService()
        {
            InitPlantMonitoringSystem()
                .ContinueWith(x => Debug.WriteLine($"{nameof(PlantMonitoringService)} Initializing!"));
        }
        private async Task SensorLoop(int milliseconds)
        {
            while (true)
            {
                 HydroponicPlantData = new HydroponicPlantData();
                foreach (MoistureSensor ms in Enum.GetValues(typeof(MoistureSensor)))
                {

                    int moistureSensor = ReadMoistureSensor(ms);
                    HydroponicPlantData.MoistureValue = moistureSensor;
                }
                HydroponicPlantData.Humidity = await _bme280Sensor.ReadHumidity();
                HydroponicPlantData.Temperature = await _bme280Sensor.ReadTemperature();
                HydroponicPlantData.LightingLevel = _mcp3008.RawAnalogResult(4);
                HydroponicPlantData.Altitude = await _bme280Sensor.ReadAltitude(1000);
                HydroponicPlantData.BarometricPressure = await _bme280Sensor.ReadPreasure();
                await Task.Delay(milliseconds);
            }


        }
        private async Task InitPlantMonitoringSystem()
        {
            try
            {
                _mcp3008 = new MCP3008();
                _bme280Sensor = new BME280Sensor();
                await _bme280Sensor.Initialize();
                _relaySensorLightPins = new List<GpioPin>(3);
                _gpioController = await GpioController.GetDefaultAsync();

                foreach (HydroPonicLights hydro in Enum.GetValues(typeof(HydroPonicLights)))
                {
                    _relaySensorLightPins.Add(
                        _gpioController.OpenPin((int)Enum.GetValues(typeof(HydroPonicLightsPinMap)).GetValue((int)hydro)));
                    _relaySensorLightPins[(int)hydro].SetDriveMode(GpioPinDriveMode.Output);
                    _relaySensorLightPins[(int)hydro].Write(GpioPinValue.Low);
                }
                HydroponicPlantData = new HydroponicPlantData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(InitPlantMonitoringSystem)} threw exception: {ex.Message}");
            }
        }

        public int ReadMoistureSensor(MoistureSensor sensor) => _mcp3008.RawAnalogResult((int)sensor);

        public void ToggleHydroponicLightOnOrOff(HydroPonicLights hydroPonicLamp)
        {
            _relaySensorLightPins[(int) hydroPonicLamp].Write(_relaySensorLightPins[(int) hydroPonicLamp].Read() ==
                                                              GpioPinValue.Low
                ? GpioPinValue.High
                : GpioPinValue.Low);
        }

        public HydroponicPlantData HydroponicPlantData { get; private set; }
    }
}