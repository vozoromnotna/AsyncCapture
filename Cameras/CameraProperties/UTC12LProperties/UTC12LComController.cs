using DirectShowLib.BDA;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace AsyncCapture.Cameras.CameraProperties.UTC12LProperties
{
    internal class UTC12LComController : COMControllerBase
    {
        private const byte DeviceAddress = 0x26;
        private const byte StartByte = 0xF0;
        private const byte EndByte = 0xFF;
        public UTC12LComController(string comPortName) : base(comPortName)
        {
        }

        protected override byte[] GetTestRequest()
        {
            return new byte[]
            {
                0xF0,
                0x02,
                0x26,
                0x00,
                0x26,
                0xFF
            };
        }

        protected override bool IsCorrect(byte[]? bytes)
        {
            throw new NotImplementedException();
        }

        protected override SerialPort OpenSerialPort(string comPortName)
        {
            return new SerialPort(comPortName, 115200);
        }

        private byte[] CreateRequest(byte instruction, byte[] data = null)
        {
            List<byte> requestData = [instruction, DeviceAddress];

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
                }
            }
            return processedData.ToArray();
        }

        public async Task SendCommand(byte instruction, byte[] data = null)
        {
            var req = CreateRequest(instruction, data);
            await SendRequest(req);
        }

        public async Task<byte[]> GetResponse(byte instruction, byte[] data = null)
        {
            var req = CreateRequest(instruction, data);
            var resp = await ReciveResponce(req);
            return resp;
        }

        public async Task FocusNear()
        {
            await SendCommand(0x01, [0x00]);
        }

        public async Task FocusFar()
        {
            await SendCommand(0x01, [0x0F]);
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

    }
}
