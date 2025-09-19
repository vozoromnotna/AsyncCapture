using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Core
{
    public class VideoFileSource : MatSource
    {
        private VideoCapture _videoCapture;

        public string Path { get; protected set; }
        public VideoFileSource(string path) 
        {
            Path = path;
            _videoCapture = new VideoCapture(path);
        }

        CancellationTokenSource _cts;
        TaskCompletionSource _tcs;
        public override void Start()
        {
            base.Start();

            _cts = new CancellationTokenSource();
            _tcs = new TaskCompletionSource();
            var token = _cts.Token;

            Task.Run(async () =>
            {
                var frame = new Mat();
                while (!token.IsCancellationRequested)
                {
                    if (_videoCapture.Read(frame))
                    {
                        var meta = new Dictionary<string, object>();
                        meta["fps"] = _videoCapture.Fps;
                        await imageGetted(frame, meta);
                    }
                    else
                    {
                        break;
                    }

                }

                _tcs.SetResult();
                RiseEndOfStream();
            }, token);
        }

        public override void Stop() 
        {
            _cts.Cancel();
        }

        public async Task WaitAsync()
        {
            await _tcs.Task;
        }
    }
}
