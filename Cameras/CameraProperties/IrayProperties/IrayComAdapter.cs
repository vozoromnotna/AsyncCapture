
using OpenCvSharp.XImgProc;
using SerialDevicesLib.Classes.SerialDeviceInfo;
using SerialDevicesLib.Classes.SerialMessages;
using SerialDevicesLib.Classes.SerialResponseValidators;
using SerialDevicesLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraProperties.IrayProperties
{
    public class IrayComDeviceInfo : ComDeviceInfo
    {
        readonly static HashSet<byte> ErrorCodes = [0xF1, 0xFB, 0xFD, 0xFF];
        public IrayComDeviceInfo() : base("IrayCom") 
        {
            _baundRate = 115200;
            _validator = new SimpleResponseValidator(Validator);
            _testValidator = new SimpleResponseValidator(Validator);
            _testRequest = new ByteSerialMessage([0xAA, 0x04, 0x01, 0x70, 0x00, 0x1F, 0xEB, 0xAA]);
            Timeout = 25;
        }

        bool Validator(ISerialMessage message)
        {
            var response = message.GetBytes();

            if (response.Length < 3)
                return false;

            if (response[0] != 0x55)
                return false;

            if (response[^1] != 0xAA)
                return false;

            if (response[^2] != 0xEB)
                return false;

            if (ErrorCodes.Contains(response[2]))
                return false;

            return true;
        }
    }

    public class IrayComAdapter
    {

        ISerialDevice _device;
        public IrayComAdapter(ISerialDevice device)
        {
            _device = device;
            _device.Open();
        }


        public int MaxZoomPosition = 2920;
        public int MinZoomPosition = 1087;

        public int MinFocusPosition = 944;
        public int MaxFocusPosition = 2776;

        SerialPort _serialPort;
        string _portName;
        const int _baudRate = 115200;

        public byte[] CreateBuffer(byte[] data)
        {
            byte[] head = { 0xAA, (byte)(data.Length + 1) };
            byte[] end = { 0xEB, 0xAA };
            var toCheck = head.Concat(data).ToArray();
            byte[] checkSum = { calculateCheckSum(toCheck) };
            return toCheck.Concat(checkSum).Concat(end).ToArray();
        }

        public async Task<byte[]> SendMessage(byte[] request)
        {
            var message = new ByteSerialMessage(request);
            var resp = await _device.SendTwoWayRequest(message);
            return resp.GetBytes();
        }

        public async void ZoomShort()
        {
            await SendMessage(new byte[] { 0xAA, 0x06, 0x08, 0x31, 0x01, 0x01, 0x00, 0xEB, 0xEB, 0xAA });
        }

        public async void ZoomLong()
        {
            await SendMessage(new byte[] { 0xAA, 0x06, 0x08, 0x31, 0x01, 0x02, 0x00, 0xEC, 0xEB, 0xAA });

        }

        public async void ZoomShutoff()
        {
            await SendMessage(new byte[] { 0xAA, 0x05, 0x08, 0x32, 0x01, 0x00, 0xEA, 0xEB, 0xAA });
        }

        public async void FocusFar()
        {
            await SendMessage(new byte[] { 0xAA, 0x06, 0x08, 0x21, 0x01, 0x02, 0x00, 0xDC, 0xEB, 0xAA });
        }

        public async void FocusNear()
        {
            await SendMessage(new byte[] { 0xAA, 0x06, 0x08, 0x21, 0x01, 0x01, 0x00, 0xDB, 0xEB, 0xAA });
        }

        public async void FocusShutoff()
        {
            await SendMessage(new byte[] { 0xAA, 0x05, 0x08, 0x22, 0x01, 0x00, 0xDA, 0xEB, 0xAA });
        }

        public async Task<int> GetFocusPosition()
        {
            var res = await SendMessage(new byte[] { 0xAA, 0x05, 0x08, 0x23, 0x00, 0x00, 0xDA, 0xEB, 0xAA });
            return BitConverter.ToInt16(res, 5);
        }

        public async Task<int> GetZoomPosition()
        {
            var res = await SendMessage(new byte[] { 0xAA, 0x05, 0x08, 0x33, 0x00, 0x00, 0xEA, 0xEB, 0xAA });
            return BitConverter.ToInt16(res, 5);

        }

        public async void AutoFocus()
        {
            var res = await SendMessage(new byte[] { 0xAA, 0x05, 0x08, 0x2F, 0x01, 0x00, 0xE7, 0xEB, 0xAA });
        }

        byte calculateCheckSum(byte[] bytes)
        {
            return bytes.Aggregate((a, b) => (byte)(a + b));
        }

    }


}
