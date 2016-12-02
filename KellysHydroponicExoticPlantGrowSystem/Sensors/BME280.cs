using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace KellysHydroponicExoticPlantGrowSystem.Sensors
{
    public class BME280_CalibrationData
    {
        //BME280 Registers
        public ushort dig_T1 { get; set; }

        public short dig_T2 { get; set; }
        public short dig_T3 { get; set; }

        public ushort dig_P1 { get; set; }
        public short dig_P2 { get; set; }
        public short dig_P3 { get; set; }
        public short dig_P4 { get; set; }
        public short dig_P5 { get; set; }
        public short dig_P6 { get; set; }
        public short dig_P7 { get; set; }
        public short dig_P8 { get; set; }
        public short dig_P9 { get; set; }

        public byte dig_H1 { get; set; }
        public short dig_H2 { get; set; }
        public byte dig_H3 { get; set; }
        public short dig_H4 { get; set; }
        public short dig_H5 { get; set; }
        public sbyte dig_H6 { get; set; }
    }

    public class BME280Sensor
    {
        //The BME280 register addresses according the the datasheet: http://www.adafruit.com/datasheets/BST-BME280-DS001-11.pdf
        private const byte BME280_Address = 0x77;

        private const byte BME280_Signature = 0x60;

        private enum eRegisters : byte
        {
            BME280_REGISTER_DIG_T1 = 0x88,
            BME280_REGISTER_DIG_T2 = 0x8A,
            BME280_REGISTER_DIG_T3 = 0x8C,

            BME280_REGISTER_DIG_P1 = 0x8E,
            BME280_REGISTER_DIG_P2 = 0x90,
            BME280_REGISTER_DIG_P3 = 0x92,
            BME280_REGISTER_DIG_P4 = 0x94,
            BME280_REGISTER_DIG_P5 = 0x96,
            BME280_REGISTER_DIG_P6 = 0x98,
            BME280_REGISTER_DIG_P7 = 0x9A,
            BME280_REGISTER_DIG_P8 = 0x9C,
            BME280_REGISTER_DIG_P9 = 0x9E,

            BME280_REGISTER_DIG_H1 = 0xA1,
            BME280_REGISTER_DIG_H2 = 0xE1,
            BME280_REGISTER_DIG_H3 = 0xE3,
            BME280_REGISTER_DIG_H4 = 0xE4,
            BME280_REGISTER_DIG_H5 = 0xE5,
            BME280_REGISTER_DIG_H6 = 0xE7,

            BME280_REGISTER_CHIPID = 0xD0,


            BME280_REGISTER_CONTROLHUMID = 0xF2,
            BME280_REGISTER_CONTROL = 0xF4,

            BME280_REGISTER_PRESSUREDATA_MSB = 0xF7,
            BME280_REGISTER_PRESSUREDATA_LSB = 0xF8,
            BME280_REGISTER_PRESSUREDATA_XLSB = 0xF9, // bits <7:4>

            BME280_REGISTER_TEMPDATA_MSB = 0xFA,
            BME280_REGISTER_TEMPDATA_LSB = 0xFB,
            BME280_REGISTER_TEMPDATA_XLSB = 0xFC, // bits <7:4>

            BME280_REGISTER_HUMIDDATA_MSB = 0xFD,
            BME280_REGISTER_HUMIDDATA_LSB = 0xFE
        }

        //String for the friendly name of the I2C bus
        private const string I2CControllerName = "I2C1";

        //Create an I2C device
        private I2cDevice _bme280;

        //Create new calibration data for the sensor
        private BME280_CalibrationData _calibrationData;

        //Variable to check if device is initialized
        private bool _init;

        //Method to initialize the BME280 sensor
        public async Task Initialize()
        {
            Debug.WriteLine("BME280::Initialize");

            try
            {
                //Instantiate the I2CConnectionSettings using the device address of the BME280
                I2cConnectionSettings settings = new I2cConnectionSettings(BME280_Address)
                {
                    BusSpeed = I2cBusSpeed.FastMode
                };
                //Set the I2C bus speed of connection to fast mode
                //Use the I2CBus device selector to create an advanced query syntax string
                string aqs = I2cDevice.GetDeviceSelector(I2CControllerName);
                //Use the Windows.Devices.Enumeration.DeviceInformation class to create a collection using the advanced query syntax string
                DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(aqs);
                //Instantiate the the BME280 I2C device using the device id of the I2CBus and the I2CConnectionSettings
                _bme280 = await I2cDevice.FromIdAsync(dis[0].Id, settings);
                //Check if device was found
                if (_bme280 == null)
                {
                    Debug.WriteLine("Device not found");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
                throw;
            }
        }

        private async Task Begin()
        {
            Debug.WriteLine("BME280::Begin");
            byte[] WriteBuffer = { (byte)eRegisters.BME280_REGISTER_CHIPID };
            byte[] ReadBuffer = { 0xFF };

            //Read the device signature
            _bme280.WriteRead(WriteBuffer, ReadBuffer);
            Debug.WriteLine("BME280 Signature: " + ReadBuffer[0]);

            //Verify the device signature
            if (ReadBuffer[0] != BME280_Signature)
            {
                Debug.WriteLine("BME280::Begin Signature Mismatch.");
                return;
            }

            //Set the initialize variable to true
            _init = true;

            //Read the coefficients table
            _calibrationData = await ReadCoefficeints();

            //Write control register
            await WriteControlRegister();

            //Write humidity control register
            await WriteControlRegisterHumidity();
        }

        //Method to write 0x03 to the humidity control register
        private async Task WriteControlRegisterHumidity()
        {
            byte[] WriteBuffer = { (byte)eRegisters.BME280_REGISTER_CONTROLHUMID, 0x03 };
            _bme280.Write(WriteBuffer);
            await Task.Delay(1);
        }

        //Method to write 0x3F to the control register
        private async Task WriteControlRegister()
        {
            byte[] WriteBuffer = { (byte)eRegisters.BME280_REGISTER_CONTROL, 0x3F };
            _bme280.Write(WriteBuffer);
            await Task.Delay(1);
        }

        //Method to read a 16-bit value from a register and return it in little endian format
        private ushort ReadLittleEndian(byte register)
        {
            ushort value;
            byte[] writeBuffer = { 0x00 };
            byte[] readBuffer = { 0x00, 0x00 };

            writeBuffer[0] = register;

            _bme280.WriteRead(writeBuffer, readBuffer);
            int h = readBuffer[1] << 8;
            int l = readBuffer[0];
            value = (ushort)(h + l);
            return value;
        }

        //Method to read an 8-bit value from a register
        private byte ReadByte(byte register)
        {
            byte value;
            byte[] writeBuffer = { 0x00 };
            byte[] readBuffer = { 0x00 };

            writeBuffer[0] = register;

            _bme280.WriteRead(writeBuffer, readBuffer);
            value = readBuffer[0];
            return value;
        }

        //Method to read the calibration data from the registers
        private async Task<BME280_CalibrationData> ReadCoefficeints()
        {
            // 16 bit calibration data is stored as Little Endian, the helper method will do the byte swap.
            _calibrationData = new BME280_CalibrationData
            {
                dig_T1 = ReadLittleEndian((byte) eRegisters.BME280_REGISTER_DIG_T1),
                dig_T2 = (short) ReadLittleEndian((byte) eRegisters.BME280_REGISTER_DIG_T2),
                dig_T3 = (short) ReadLittleEndian((byte) eRegisters.BME280_REGISTER_DIG_T3),
                dig_P1 = ReadLittleEndian((byte) eRegisters.BME280_REGISTER_DIG_P1),
                dig_P2 = (short) ReadLittleEndian((byte) eRegisters.BME280_REGISTER_DIG_P2),
                dig_P3 = (short) ReadLittleEndian((byte) eRegisters.BME280_REGISTER_DIG_P3),
                dig_P4 = (short) ReadLittleEndian((byte) eRegisters.BME280_REGISTER_DIG_P4),
                dig_P5 = (short) ReadLittleEndian((byte) eRegisters.BME280_REGISTER_DIG_P5),
                dig_P6 = (short) ReadLittleEndian((byte) eRegisters.BME280_REGISTER_DIG_P6),
                dig_P7 = (short) ReadLittleEndian((byte) eRegisters.BME280_REGISTER_DIG_P7),
                dig_P8 = (short) ReadLittleEndian((byte) eRegisters.BME280_REGISTER_DIG_P8),
                dig_P9 = (short) ReadLittleEndian((byte) eRegisters.BME280_REGISTER_DIG_P9),
                dig_H1 = ReadByte((byte) eRegisters.BME280_REGISTER_DIG_H1),
                dig_H2 = (short) ReadLittleEndian((byte) eRegisters.BME280_REGISTER_DIG_H2),
                dig_H3 = ReadByte((byte) eRegisters.BME280_REGISTER_DIG_H3),
                dig_H4 =
                    (short)
                    ((ReadByte((byte) eRegisters.BME280_REGISTER_DIG_H4) << 4) |
                     (ReadByte((byte) eRegisters.BME280_REGISTER_DIG_H4 + 1) & 0xF)),
                dig_H5 =
                    (short)
                    ((ReadByte((byte) eRegisters.BME280_REGISTER_DIG_H5 + 1) << 4) |
                     (ReadByte((byte) eRegisters.BME280_REGISTER_DIG_H5) >> 4)),
                dig_H6 = (sbyte) ReadByte((byte) eRegisters.BME280_REGISTER_DIG_H6)
            };

            // Read temperature calibration data

            // Read presure calibration data

            // Read humidity calibration data

            await Task.Delay(1);
            return _calibrationData;
        }

        //t_fine carries fine temperature as global value
        private int _fine = int.MinValue;

        //Method to return the temperature in DegC. Resolution is 0.01 DegC. Output value of “5123” equals 51.23 DegC.
        private double BME280_compensate_T_double(int adc_T)
        {
            double var1, var2, T;

            //The temperature is calculated using the compensation formula in the BME280 datasheet
            var1 = ((adc_T / 16384.0) - (_calibrationData.dig_T1 / 1024.0)) * _calibrationData.dig_T2;
            var2 = ((adc_T / 131072.0) - (_calibrationData.dig_T1 / 8192.0)) * _calibrationData.dig_T3;

            _fine = (int)(var1 + var2);

            T = (var1 + var2) / 5120.0;
            return T;
        }

        //Method to returns the pressure in Pa, in Q24.8 format (24 integer bits and 8 fractional bits).
        //Output value of “24674867” represents 24674867/256 = 96386.2 Pa = 963.862 hPa
        private long BME280_compensate_P_Int64(int adc_P)
        {
            long var1, var2, p;

            //The pressure is calculated using the compensation formula in the BME280 datasheet
            var1 = _fine - 128000;
            var2 = var1 * var1 * _calibrationData.dig_P6;
            var2 = var2 + ((var1 * _calibrationData.dig_P5) << 17);
            var2 = var2 + ((long)_calibrationData.dig_P4 << 35);
            var1 = ((var1 * var1 * _calibrationData.dig_P3) >> 8) + ((var1 * _calibrationData.dig_P2) << 12);
            var1 = ((((long)1 << 47) + var1) * _calibrationData.dig_P1) >> 33;
            if (var1 == 0)
            {
                Debug.WriteLine("BME280_compensate_P_Int64 Jump out to avoid / 0");
                return 0; //Avoid exception caused by division by zero
            }
            //Perform calibration operations as per datasheet: http://www.adafruit.com/datasheets/BST-BME280-DS001-11.pdf
            p = 1048576 - adc_P;
            p = (((p << 31) - var2) * 3125) / var1;
            var1 = (_calibrationData.dig_P9 * (p >> 13) * (p >> 13)) >> 25;
            var2 = (_calibrationData.dig_P8 * p) >> 19;
            p = ((p + var1 + var2) >> 8) + ((long)_calibrationData.dig_P7 << 4);
            return p;
        }

        // Returns humidity in %RH as unsigned 32 bit integer in Q22.10 format (22 integer and 10
        // fractional bits). Output value of “47445” represents 47445/1024 = 46.333 %RH
        private uint BME280_compensate_H_int32(int adc_H)
        {
            int v_x1_u32r;
            v_x1_u32r = (_fine - 76800);
            v_x1_u32r = (((((adc_H << 14) - (_calibrationData.dig_H4 << 20) - (_calibrationData.dig_H5 * v_x1_u32r)) +
            16384) >> 15) * (((((((v_x1_u32r * _calibrationData.dig_H6) >> 10) * (((v_x1_u32r *
                _calibrationData.dig_H3) >> 11) + 32768)) >> 10) + 2097152) *
            _calibrationData.dig_H2 + 8192) >> 14));
            v_x1_u32r = (v_x1_u32r - (((((v_x1_u32r >> 15) * (v_x1_u32r >> 15)) >> 7) * _calibrationData.dig_H1) >> 4));
            v_x1_u32r = (v_x1_u32r < 0 ? 0 : v_x1_u32r);
            v_x1_u32r = (v_x1_u32r > 419430400 ? 419430400 : v_x1_u32r);
            return (uint)(v_x1_u32r >> 12);
        }

        public async Task<float> ReadTemperature()
        {
            //Make sure the I2C device is initialized
            if (!_init) await Begin();

            //Read the MSB, LSB and bits 7:4 (XLSB) of the temperature from the BME280 registers
            byte tmsb = ReadByte((byte)eRegisters.BME280_REGISTER_TEMPDATA_MSB);
            byte tlsb = ReadByte((byte)eRegisters.BME280_REGISTER_TEMPDATA_LSB);
            byte txlsb = ReadByte((byte)eRegisters.BME280_REGISTER_TEMPDATA_XLSB); // bits 7:4

            //Combine the values into a 32-bit integer
            int t = (tmsb << 12) + (tlsb << 4) + (txlsb >> 4);

            //Convert the raw value to the temperature in degC
            double temp = BME280_compensate_T_double(t);

            //Return the temperature as a float value
            return (float)temp;
        }

        public async Task<float> ReadPreasure()
        {
            //Make sure the I2C device is initialized
            if (!_init) await Begin();

            //Read the temperature first to load the t_fine value for compensation
            if (_fine == int.MinValue)
            {
                await ReadTemperature();
            }

            //Read the MSB, LSB and bits 7:4 (XLSB) of the pressure from the BME280 registers
            byte tmsb = ReadByte((byte)eRegisters.BME280_REGISTER_PRESSUREDATA_MSB);
            byte tlsb = ReadByte((byte)eRegisters.BME280_REGISTER_PRESSUREDATA_LSB);
            byte txlsb = ReadByte((byte)eRegisters.BME280_REGISTER_PRESSUREDATA_XLSB); // bits 7:4

            //Combine the values into a 32-bit integer
            int t = (tmsb << 12) + (tlsb << 4) + (txlsb >> 4);

            //Convert the raw value to the pressure in Pa
            long pres = BME280_compensate_P_Int64(t);

            //Return the temperature as a float value
            return (float)pres / 256;
        }

        public async Task<float> ReadHumidity()
        {
            if (!_init) await Begin();

            byte tmsb = ReadByte((byte)eRegisters.BME280_REGISTER_HUMIDDATA_MSB);
            byte tlsb = ReadByte((byte)eRegisters.BME280_REGISTER_HUMIDDATA_LSB);
            int uncompensated = (tmsb << 8) + tlsb;
            uint humidity = BME280_compensate_H_int32(uncompensated);

            return ((float)humidity) / 1000;
        }

        //Method to take the sea level pressure in Hectopascals(hPa) as a parameter and calculate the altitude using current pressure.
        public async Task<float> ReadAltitude(float seaLevel)
        {
            //Make sure the I2C device is initialized
            if (!_init) await Begin();

            //Read the pressure first
            float pressure = await ReadPreasure();
            //Convert the pressure to Hectopascals(hPa)
            pressure /= 100;

            //Calculate and return the altitude using the international barometric formula
            return 44330.0f * (1.0f - (float)Math.Pow((pressure / seaLevel), 0.1903f));
        }
    }
}