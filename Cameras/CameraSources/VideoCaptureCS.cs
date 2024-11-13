using DirectShowLib;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AsyncCapture.Cameras.CameraSources;

public class VideoCaptureCS : CameraSource
{
    public VideoCaptureCS(VideoCapture videoCapture, string name)
    {
        _name = name;
        _videoCapture = videoCapture;

        startLive();
    }

    protected void stopLive()
    {
        _videoCapture.Dispose();
    }

    Thread _captureThread;
    protected void startLive()
    {
        _captureThread = new Thread(async () =>
        {
            while (true)
            {
                if (!_isLive)
                    continue;

                var frame = new Mat();
                if (_videoCapture.Read(frame))
                {
                    var meta = new Dictionary<string, object>();
                    await imageGetted(frame, meta);
                    
                }
                else
                {
					break;
				}
                    
            }

            RiseEndOfStream();
        });

        _captureThread.Start();
    }

    readonly string _name;
    public override string Name => _name;

    protected bool _isLive = true;
    public override bool IsLive => _isLive;

    public override void Dispose()
    {
        _isLive = false;
        _videoCapture.Release();
        _videoCapture.Dispose();
    }

    public override void StartLive()
    {
        _isLive = true;
    }

    public override void StopLive()
    {
        _isLive = false;
    }

    private VideoCapture _videoCapture;
}
