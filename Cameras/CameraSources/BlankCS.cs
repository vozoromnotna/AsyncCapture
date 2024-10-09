

using AsyncCapture.Cameras.CameraProperties;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AsyncCapture.Cameras.CameraSources;

public class BlankCS : CameraSource
{
    private static int blankCount = 1;

    private string name;
    public override string Name { get => name; }

    private bool _isLive = true;
    public override bool IsLive => _isLive;

    public BlankCS()
    {
        name = $"Blank_{blankCount}";
        blankCount++;

        Properties.Add(new AutoExpBlank());
        Properties.Add(new ExpBlank(Properties[0] as AutoExpBlank));
        var gainProp = new GainBlank();
        Properties.Add(gainProp);
        Properties.Add(new ImgFormatBlank());
        Properties.Add(new DoubleToPecloWrapper(gainProp, "GainPelco", "Усиление 2", 0, 100));

        startImageThread();
    }

    CancellationTokenSource _cts;
    public void startImageThread()
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                if (!_isLive)
                    continue;

                var img = CreateBlankImage();
                var meta = new Dictionary<string, object>();
                imageGetted(img, meta);
                await Task.Delay(100);
            }
        }, token);
    }

    private Mat CreateBlankImage()
    {
        var img = new Mat(100, 100, MatType.CV_8UC3);
        Cv2.Randu(img, Scalar.Black, Scalar.White);
        return img;
    }

    public override void Dispose()
    {
        _cts.Cancel();
    }

    public override void StartLive()
    {
        _isLive = true;
    }

    public override void StopLive()
    {
        _isLive = false;
    }
}
