
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
    public class IrayCOMControl
    {

        public IrayCOMControl(string portName, int baundRate)
        {
            _portName = portName;
            _baudRate = baundRate;
            _serialPort = new SerialPort(_portName, _baudRate);
            _serialPort.Open();
        }


        public int MaxZoomPosition = 2920;
        public int MinZoomPosition = 1087;

        public int MinFocusPosition = 944;
        public int MaxFocusPosition = 2776;

        SerialPort _serialPort;
        string _portName;
        int _baudRate;

        public byte[] CreateBuffer(byte[] data)
        {
            byte[] head = { 0xAA, (byte)(data.Length + 1) };
            byte[] end = { 0xEB, 0xAA };
            var toCheck = head.Concat(data).ToArray();
            byte[] checkSum = { calculateCheckSum(toCheck) };
            return toCheck.Concat(checkSum).Concat(end).ToArray();
        }

        private void writeBuffer(byte[] buffer)
        {

            if (_serialPort == null || !_serialPort.IsOpen)
                return;

            _serialPort.Write(buffer, 0, buffer.Length);
        }

        readonly int _serialTimeout = 25;
        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        const int MaxTries = 3;
        public async Task<byte[]> SendMessage(byte[] buffer)
        {
            int tries = 0;
            await _semaphore.WaitAsync();
            var result = new List<byte>();
            while(true)
            {
                result.Clear();

                _serialPort.Write(buffer, 0, buffer.Length);

                await Task.Delay(_serialTimeout);

                while (_serialPort.BytesToRead > 0)
                {
                    var readRes = _serialPort.ReadByte();
                    if (readRes > 0)
                    {
                        result.Add((byte)readRes);
                    }
                    else
                    {
                        break;
                    }

                }

                if (!IsCorrect(result) && tries < MaxTries)
                {
                    tries++;
                    await Task.Delay(_serialTimeout);
                }
                else
                {
                    break;
                }

            } 

            _semaphore.Release();
            return result.ToArray();
        }

        readonly HashSet<byte> ErrorCodes = [0xF1, 0xFB, 0xFD, 0xFF];
        private bool IsCorrect(List<byte> response)
        {
            if (response.Count < 3)
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
