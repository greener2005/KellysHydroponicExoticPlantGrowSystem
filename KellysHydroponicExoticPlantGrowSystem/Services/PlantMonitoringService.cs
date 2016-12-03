using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using KellysHydroponicExoticPlantGrowSystem.Enums;
using KellysHydroponicExoticPlantGrowSystem.Interfaces;
using KellysHydroponicExoticPlantGrowSystem.Models;
using KellysHydroponicExoticPlantGrowSystem.Sensors;

namespace KellysHydroponicExoticPlantGrowSystem.Services
{
    public class PlantMonitoringService : IPlantMonitoringService
    {
        private const int LIGHT_SENSOR_CHANNEL = 3;
        private BMP280Sensor33 _bme280Sensor;
        private GpioController _gpioController;
        private MCP3008 _mcp3008;
        private List<GpioPin> _relaySensorLightPins;
        private DhtTemeratureSensor _dhtTemeratureSensor;


        public PlantMonitoringService()
        {
            _mcp3008 = new MCP3008();
            _dhtTemeratureSensor = new DhtTemeratureSensor();

            initAnalogDevice().ContinueWith(y =>
            {
                InitPlantMonitoringSystem()
                    .ContinueWith(x =>
                    {
                        Debug.WriteLine($"{nameof(PlantMonitoringService)} Initialize and starting telemtry!");
                        Task.Factory.StartNew(() => SensorLoopAsync(500));
                    });
            });


        }
        

        public int ReadMoistureSensor(MoistureSensor sensor) => _mcp3008.RawAnalogResult((int) sensor);

        public void ToggleHydroponicLightOnOrOff(HydroPonicLights hydroPonicLamp)
        {
            _relaySensorLightPins[(int) hydroPonicLamp].Write(_relaySensorLightPins[(int) hydroPonicLamp].Read() ==
                                                              GpioPinValue.Low
                ? GpioPinValue.High
                : GpioPinValue.Low);
        }

        public HydroponicPlantData HydroponicPlantData { get; private set; }

        private async Task SensorLoopAsync(int milliseconds)
        {
            while (true)
            {
                HydroponicPlantData = new HydroponicPlantData();
                foreach (MoistureSensor ms in Enum.GetValues(typeof(MoistureSensor)))
                {
                    var moistureSensor = ReadMoistureSensor(ms);
                    HydroponicPlantData.MoistureValue = moistureSensor;
                }
                // HydroponicPlantData.Humidity = _bme280Sensor.Humidity;
                HydroponicPlantData.LightingLevel = _mcp3008.RawAnalogResult(LIGHT_SENSOR_CHANNEL);
                //HydroponicPlantData.Altitude = await _bme280Sensor.ReadAltitude(SEA_LEVEL_LAKEVILLE_MN);
                //HydroponicPlantData.BarometricPressure = await _bme280Sensor.ReadPreasure();


                var sensorData = await _bme280Sensor.GetSensorDataAsync(Bmp180AccuracyMode.UltraHighResolution);
                var temperatureText = sensorData.TemperatureFarenheight.ToString("F1");
                var pressureText = sensorData.Pressure.ToString("F2");
                //temperatureText += "C - hex:" + BitConverter.ToString(sensorData.UncompestatedTemperature);
                //pressureText += "hPa - hex:" + BitConverter.ToString(sensorData.UncompestatedPressure);
                Debug.WriteLine($"Temp: {temperatureText}\r\n Barometer: {pressureText}\r\n LightLevel: {HydroponicPlantData.LightingLevel}");

                await Task.Delay(milliseconds);
            }
        }

        private async Task initAnalogDevice()
        {
            try
            {
               
                await _mcp3008.InitAsync().ContinueWith(x => "Init MCP3008");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(initAnalogDevice)} threw this exception: {ex.Message}");
            }
        }
        private async Task InitPlantMonitoringSystem()
        {
            try
            {

                _bme280Sensor = new BMP280Sensor33();
                await _bme280Sensor.InitializeAsync();
                _relaySensorLightPins = new List<GpioPin>(3);
                _gpioController = await GpioController.GetDefaultAsync();

                foreach (HydroPonicLights hydro in Enum.GetValues(typeof(HydroPonicLights)))
                {
                    _relaySensorLightPins.Add(
                        _gpioController.OpenPin(
                            (int) Enum.GetValues(typeof(HydroPonicLightsPinMap)).GetValue((int) hydro)));
                    _relaySensorLightPins[(int) hydro].SetDriveMode(GpioPinDriveMode.Output);
                    _relaySensorLightPins[(int) hydro].Write(GpioPinValue.Low);
                }

                HydroponicPlantData = new HydroponicPlantData();

                _dhtTemeratureSensor.RunDHTSensor(21,_gpioController,TimeSpan.FromMilliseconds(1000));
                _dhtTemeratureSensor.DhtValuesChanged += OnDhtTemperatureChange;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(InitPlantMonitoringSystem)} threw exception: {ex.Message}");
            }
        }

        private void OnDhtTemperatureChange(object sender, DHTTempArgs e)
        {
            if (HydroponicPlantData != null)
            {
                HydroponicPlantData.Humidity = e.Humid;
                HydroponicPlantData.DewPoint = e.DewPoint;
                HydroponicPlantData.HeatIndex = e.HeatIndex;
                Debug.WriteLine($"Humidity: {e.Humid}");
            }
        }
        ~PlantMonitoringService()
        {
            if (_dhtTemeratureSensor != null)
                _dhtTemeratureSensor.DhtValuesChanged -= OnDhtTemperatureChange;
        }
    }
}