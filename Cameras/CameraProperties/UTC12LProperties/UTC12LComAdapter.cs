using DirectShowLib.BDA;
using SerialDevicesLib.Classes.Devices;
using SerialDevicesLib.Classes.SerialDeviceInfo;
using SerialDevicesLib.Classes.SerialMessages;
using SerialDevicesLib.Classes.SerialResponseValidators;
using SerialDevicesLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace AsyncCapture.Cameras.CameraProperties.UTC12LProperties
{
    public class UTC12LComDeviceInfo : ComDeviceInfo
    {

        public UTC12LComDeviceInfo() : base("UTC12LCom")
        {
            _baundRate = 115200;
            _validator = new SimpleResponseValidator(Validator);
            _testValidator = new SimpleResponseValidator(TestValidator);
            _testRequest = new ByteSerialMessage([0xF0, 0x02, 0x26, 0x00, 0x26, 0xFF]);
            Timeout = 200;
        }

        bool Validator(ISerialMessage message)
        {
            var bytes = message.GetBytes();

            if (bytes.Length < 3)
                return false;

            if (bytes[0] != 0xF0)
                return false;

            if (bytes[^1] != 0xFF)
                return false;

            return true;
        }

        bool TestValidator(ISerialMessage message) 
        {
            var bytes = message.GetBytes();

            if (bytes.Length < 22)
                return false;

            if (bytes[0] != 0xF0)
                return false;

            if (bytes[^1] != 0xFF)
                return false;

            if (bytes[1] == 0x02 && bytes[2] == 0x26 && bytes[3] == 0x00 && bytes[4] == 0x026)
                return false;

            return true;
        }

    }
    public class UTC12LComAdapter
    {
        private const byte DeviceAddress = 0x26;
        private const byte StartByte = 0xF0;
        private const byte EndByte = 0xFF;
        ISerialDevice _comDevice;
        public UTC12LComAdapter(ISerialDevice comDevice)
        {
            _comDevice = comDevice;
            _comDevice.Open();
        }

        private byte[] CreateRequest(byte instruction, byte[] data = null)
        {
            List<byte> requestData = [DeviceAddress, instruction];

            if (data != null)
            {
                data = ProcessData(data);
                requestData.AddRange(data);
            }

            byte checkSum = CalculateCheckSum(requestData);
            List<byte> request = [StartByte, (byte)requestData.Count];
            request.AddRange(requestData);
            request.Add(checkSum);
            request.Add(EndByte);

            return request.ToArray();
        }

        private byte CalculateCheckSum(List<byte> data)
        {
            return (byte)data.Sum(x => x);
        }

        private byte[] ProcessData(byte[] data) 
        {
            List<byte> processedData = new();
            foreach (byte b in data)
            {
                switch (b)
                {
                    case 0xF0:
                        processedData.Add(0xF5);
                        processedData.Add(0x00);
                        break;
                    case 0xFF:
                        processedData.Add(0xF5);
                        processedData.Add(0x0F);
                        break;
                    case 0xF5:
                        processedData.Add(0xF5);
                        processedData.Add(0x05);
                        break;
                    default:
                        processedData.Add(b);
                        break;
                }
            }
            return processedData.ToArray();
        }

        public async Task SendCommand(byte instruction, byte[] data = null)
        {
            var req = CreateRequest(instruction, data);
            var message = new ByteSerialMessage(req);
            await _comDevice.SendOneWayRequest(message);
        }

        public async Task<byte[]> GetResponse(byte instruction, byte[] data = null)
        {
            var req = CreateRequest(instruction, data);
            var message = new ByteSerialMessage(req);
            var resp = await _comDevice.SendTwoWayRequest(message);
            return resp.GetBytes();
        }

        public async Task FocusNear()
        {
            await SendCommand(0x01, [0x00]);
        }

        public async Task FocusFar()
        {
            await SendCommand(0x01, [0x0F]);
        }

        public async Task FocusStop()
        {
            await SendCommand(0x10, []);
        }

        public async Task ZoomBig()
        {
           await SendCommand(0x11, [0x00]);
        }

        public async Task ZoomSmall()
        {
            await SendCommand(0x11, [0x0F]);
        }

        public async Task AutoFocus()
        {
            await SendCommand(0x34);
        }

        public async Task SetGain(byte gain)
        {
            await SendCommand(0x09, [gain]);
        }

        public async Task SetBrightness(byte brightness)
        {
            await SendCommand(0x0A, [brightness]);
        }

        public async Task<byte[]> GetStatus()
        {
            return await GetResponse(0x00);
        }

        public async Task ImageEnhancment(bool status)
        {
            byte[] data = [(byte)(status ? 0x0F : 0x00)]; 
            await SendCommand(0x0E, data);
        }

        public async Task TimeDomainFilter(bool status)
        {
            byte[] data = [(byte)(status ? 0x0F : 0x00)];
            await SendCommand(0x0D, data);
        }

        public async Task WhiteHot(bool status)
        {
            byte[] data = [(byte)(status ? 0x0F : 0x00)];
            await SendCommand(0x05, data);
        }

        public async Task AutoCalibration(bool status)
        {
            byte[] data = [(byte)(status ? 0x0F : 0x00)];
            await SendCommand(0x07, data);
        }
    }
}
