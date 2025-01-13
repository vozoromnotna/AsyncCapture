using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace AsyncCapture.Cameras.CameraProperties
{
    public abstract class COMControllerBase
    {
        const int _serialTimeout = 25;
        static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        const int MaxTries = 3;
        SerialPort _serialPort;

        public COMControllerBase(string comPortName)
        {
           _serialPort = OpenSerialPort(comPortName);
        }

        protected abstract SerialPort OpenSerialPort(string comPortName);
        public abstract byte[] GetTestRequest();
        public virtual async Task<bool> TestPort()
        {
            var request = GetTestRequest();
            var responce  = await ReciveResponce(request);
            return IsCorrect(responce);

        }
        public async Task SendRequest(byte[] request)
        {
            await SendRequest(_serialPort, request);
        }

        public async Task<byte[]> ReciveResponce(byte[] request)
        {
            return await ReciveResponce(_serialPort, request);
        }
        public async Task<byte[]> ReciveResponce(SerialPort serialPort, byte[] message)
        {
            int tries = 0;
            await _semaphore.WaitAsync();
            var result = new List<byte>();
            while (true)
            {
                result.Clear();
                serialPort.Write(message, 0, message.Length);

                await Task.Delay(_serialTimeout);

                while (serialPort.BytesToRead > 0)
                {
                    var readRes = serialPort.ReadByte();
                    if (readRes > -1)
                    {
                        result.Add((byte)readRes);
                    }
                    else
                    {
                        break;
                    }

                }

                if (!IsCorrect(result?.ToArray()) && tries < MaxTries)
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

        protected abstract bool IsCorrect(byte[]? bytes);

        public async Task SendRequest(SerialPort serialPort, byte[] message)
        {
            await _semaphore.WaitAsync();
            var result = new List<byte>();

            result.Clear();

            serialPort.Write(message, 0, message.Length);

            await Task.Delay(_serialTimeout);

            _semaphore.Release();
        }

    }
}
