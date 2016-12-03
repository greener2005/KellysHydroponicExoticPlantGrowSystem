using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;

namespace KellysHydroponicExoticPlantGrowSystem.Sensors
{
    public sealed class MCP3008
    {
        private SpiDevice _spiDevice;

        public async Task InitAsync()
        {
            var spiQuery = SpiDevice.GetDeviceSelector("SPI0");

            var deviceResults = await DeviceInformation.FindAllAsync(spiQuery, null);

            if (deviceResults != null && deviceResults.Count > 0)
                try
                {
                    var spiSettings = new SpiConnectionSettings(0)
                    {
                        //3.6 MHz at 50
                        ClockFrequency = 3600000,
                        Mode = SpiMode.Mode0,
                        SharingMode = SpiSharingMode.Exclusive
                    };

                    _spiDevice = await SpiDevice.FromIdAsync(deviceResults[0].Id, spiSettings);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(InitAsync)} in MCP3008 threw this exception: {ex.Message}");
                }
            else
                Debug.WriteLine("SPI device not found!");
        }

        public int RawAnalogResult(int channel)
        {
            if (_spiDevice == null)
                return -1;

            var channelBit = (8 + channel) << 4;

            byte[] transmitBuffer = {1, (byte) channelBit, 0};
            byte[] receiveBuffer = {0, 0, 0};
            _spiDevice.TransferFullDuplex(transmitBuffer, receiveBuffer);
            return ((receiveBuffer[1] & 3) << 8) + receiveBuffer[2];
        }
    }
}