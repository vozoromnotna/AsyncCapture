using Gst;
using OpenCvSharp;
using ScottPlot.Drawing.Colormaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Task = System.Threading.Tasks.Task;

namespace AsyncCapture.Cameras.CameraSources
{
    public class TestCS : CameraSource
    {
        public TestCS()
        {

        }

        public int Width { get; set; } = 1920;
        public int Height { get; set; } = 1080;
        public double Fps { get; set; } = 120.0;
        public MatType MatType { get; set; } = MatType.CV_8UC1;
        public override string Name => "TestSource";

        private bool _isLive = true;
        public override bool IsLive { get => _isLive; }


        double[] _pregenValues;
        private void PregenValues(int heigth, double maxIntensity)
        {
            _pregenValues = new double[heigth * 2];
            for (int i = 0; i < _pregenValues.Length; i++)
            {
                _pregenValues[i] = (Math.Cos(Math.PI * (i) / Height) + 1) * maxIntensity / 2.0;
            }
        }
        private double GetValue(int phase, int height)
        {
            var curPhase = phase % (2 * height);
            return _pregenValues[curPhase];
        }

        CancellationTokenSource _cts;
        public void StartImage()
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            Task.Run(async () =>
            {
                int r = 1;
                int increment = 5;
                int directionX = 1;
                int xSpeed = 10;
                int directionY = 1;
                int ySpeed = 8;
                int phase = 0;
                int phaseInc = 10;
                int color = 0;
                int colorMaxCount = (int)Fps;
                int colorCount = 0;
                int x = Height / 2;
                int y = Width / 2;
                var channels = MatType.Channels;
                var depth = MatType.Depth;
                var maxIntensity = Math.Pow(2, (depth / channels) * 8) - 1;
                double[,] imageData = new double[Height, Width];

                PregenValues(Height, maxIntensity);
                while (!token.IsCancellationRequested)
                {
                    var timeout = (int)((1.0 / Fps) * 1000);
                    var waitTask = Task.Delay(timeout);
                    
                    Parallel.For(0, Height, i =>
                    {
                        var value = GetValue(i + phase, Height);
                        for (int j = 0; j < Width; j++)
                        {
                            imageData[i, j] = value;
                        }
                    });

                    var image = Mat.FromArray(imageData);
                    image.ConvertTo(image, MatType);
                    if (channels == 3)
                    {
                        Cv2.CvtColor(image, image, ColorConversionCodes.GRAY2BGR);
                    }
                    

                    phase += phaseInc;

                    Scalar colorScalar = Scalar.All(maxIntensity);

                    if (MatType == MatType.CV_8UC3)
                    {
                        var blue = (color & 0xFF0000) >> 16;
                        var green = (color & 0x00FF00) >> 8;
                        var red = color & 0x0000FF;

                        colorCount += 1;
                        if (colorCount > colorMaxCount)
                        {
                            color = new Random().Next(0xFFFFFF);
                        }
                        colorScalar = Scalar.FromRgb(red, green, blue);
                    }

                    Cv2.Circle(image, new Point(x, y), r, colorScalar, -1);

                    if (x + r >= image.Cols)
                    {
                        directionX = -1;
                    }

                    if (x - r <= 0)
                    {
                        directionX = 1;
                    }

                    if (y + r >= image.Rows)
                    {
                        directionY = -1;
                    }

                    if (y - r <= 0)
                    {
                        directionY = 1;
                    }

                    x += directionX * xSpeed;
                    y += directionY * ySpeed;
                    r += increment;
                    if (r > 100 || r <= Math.Abs(increment))
                    {
                        increment *= -1;
                    }

                    Dictionary<string, object> meta = new();

                    Task.WaitAll(waitTask, imageGetted(image, meta));
                }

            }, token);
        }


        public override void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public override void StartLive()
        {
            _isLive = true;
            StartImage();
        }

        public override void StopLive()
        {
            _isLive = false;
            _cts?.Cancel();
        }
    }
}
