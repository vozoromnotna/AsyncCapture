
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
            _serialPort.ReadTimeout = 300;
            _serialPort.WriteTimeout = 300;
            _serialPort.Open();
        }

        const string DefaultPort = "COM26";
        const int DefaultBandRate = 115200;

        public int MaxZoomPosition = 2920;
        public int MinZoomPosition = 1087;

        public int MinFocusPosition = 944;
        public int MaxFocusPosition = 2776;

        SerialPort _serialPort;
        string _portName = DefaultPort;
        int _baudRate = DefaultBandRate;

        private byte[] createBuffer(byte[] data)
        {
            byte[] head = { 0xAA, (byte)(data.Length + 1) };
            byte[] end = { 0xEB, 0xAA };
            byte[] checkSum = { calculateCheckSum(data) };
            return head.Concat(data).Concat(checkSum).Concat(end).ToArray();
        }

        private void writeBuffer(byte[] buffer)
        {

            if (_serialPort == null || !_serialPort.IsOpen)
                return;

            _serialPort.Write(buffer, 0, buffer.Length);
        }

        object _serialLocker = new object();
        public async Task<byte[]> sendMessage(byte[] buffer)
        {
            using (SemaphoreSlim semaphore = new SemaphoreSlim(0, 1))
            {
                byte[] readBuffer = null;
                SerialDataReceivedEventHandler handler = (sender, e) =>
                {
                    int bytes = _serialPort.BytesToRead;
                    if (bytes > 0)
                    {
                        readBuffer = new byte[bytes];
                        _serialPort.Read(readBuffer, 0, bytes);
                        semaphore.Release();
                    }
                };

                try
                {
                    _serialPort.DataReceived += handler;
                    writeBuffer(buffer);

                    semaphore.Wait(1000);
                }
                finally
                {
                    _serialPort.DataReceived -= handler;
                }

                return readBuffer;
            }
        }

        public async void ZoomShort()
        {
            await sendMessage(new byte[] { 0xAA, 0x06, 0x08, 0x31, 0x01, 0x01, 0x00, 0xEB, 0xEB, 0xAA });
        }

        public async void ZoomLong()
        {
            await sendMessage(new byte[] { 0xAA, 0x06, 0x08, 0x31, 0x01, 0x02, 0x00, 0xEC, 0xEB, 0xAA });

        }

        public async void ZoomShutoff()
        {
            await sendMessage(new byte[] { 0xAA, 0x05, 0x08, 0x32, 0x01, 0x00, 0xEA, 0xEB, 0xAA });
        }

        public async void FocusFar()
        {
            await sendMessage(new byte[] { 0xAA, 0x06, 0x08, 0x21, 0x01, 0x02, 0x00, 0xDC, 0xEB, 0xAA });
        }

        public async void FocusNear()
        {
            await sendMessage(new byte[] { 0xAA, 0x06, 0x08, 0x21, 0x01, 0x01, 0x00, 0xDB, 0xEB, 0xAA });
        }

        public async void FocusShutoff()
        {
            await sendMessage(new byte[] { 0xAA, 0x05, 0x08, 0x22, 0x01, 0x00, 0xDA, 0xEB, 0xAA });
        }

        public async Task<int> GetFocusPosition()
        {
            var res = await sendMessage(new byte[] { 0xAA, 0x05, 0x08, 0x23, 0x00, 0x00, 0xDA, 0xEB, 0xAA });
            return BitConverter.ToInt16(res, 5);
        }

        public async Task<int> GetZoomPosition()
        {
            var res = await sendMessage(new byte[] { 0xAA, 0x05, 0x08, 0x33, 0x00, 0x00, 0xEA, 0xEB, 0xAA });
            return BitConverter.ToInt16(res, 5);

        }

        public async void AutoFocus()
        {
            var res = await sendMessage(new byte[] { 0xAA, 0x05, 0x08, 0x2F, 0x01, 0x00, 0xE7, 0xEB, 0xAA });
        }

        byte calculateCheckSum(byte[] bytes)
        {
            return bytes.Aggregate((a, b) => (byte)(a + b));
        }

    }


}
