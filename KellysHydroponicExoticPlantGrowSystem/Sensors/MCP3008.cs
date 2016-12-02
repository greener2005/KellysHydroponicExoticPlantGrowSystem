using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;

namespace KellysHydroponicExoticPlantGrowSystem.Sensors
{
    public sealed class MCP3008
    {
        private SpiDevice _mcp3008;

        public MCP3008()
        {
            Init().ContinueWith(x => Debug.WriteLine("SPI Init"));
        }

        private async Task Init()
        {
            var spiSettings = new SpiConnectionSettings(0)
            {
                //3.6 MHz at 5v
                ClockFrequency = 3600000,
                Mode = SpiMode.Mode3
            };

            var spiQuery = SpiDevice.GetDeviceSelector("SPI0");

            var devices = DeviceInformation.FindAllAsync(spiQuery, null);

            var deviceResults = devices?.GetResults();

            if (deviceResults != null && deviceResults.Count > 0)
            {
                _mcp3008 = await SpiDevice.FromIdAsync(deviceResults[0].Id, spiSettings);
            }
            else
            {
                Debug.WriteLine("SPI device not found!");
            }
        }

        public int RawAnalogResult(int channel)
        {
            if (_mcp3008 == null)
                return -1;

            int channelBit = 8 + channel << 4;

            byte[] transmitBuffer = { 1, (byte)channelBit, 0 };
            byte[] receiveBuffer = { 0, 0, 0 };
            _mcp3008.TransferFullDuplex(transmitBuffer, receiveBuffer);
            return ((receiveBuffer[1] & 3) << 8) + receiveBuffer[2];
        }
    }
}