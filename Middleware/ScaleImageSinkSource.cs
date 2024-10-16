using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Middleware;

public class ScaleImageSinkSource : MatSource, ISinkSource<Mat>
{
    private double _hScaleFactor;
    private double _wScaleFactor;

    public double HeightScaleFactor { get => _hScaleFactor; }
    public double WidthScaleFactor {  get => _wScaleFactor; }
    public ScaleImageSinkSource(double widthScaleFactor, double heightScaleFactor) 
    {
        _hScaleFactor = heightScaleFactor;
        _wScaleFactor = widthScaleFactor;
    }
    public async Task PutImage(Mat image, Dictionary<string, object> meta)
    {
        Mat scaledImage = new();
        OpenCvSharp.Size newSize = new OpenCvSharp.Size(image.Width * _wScaleFactor, image.Height * _hScaleFactor);
        Cv2.Resize(image, scaledImage, newSize);

        await imageGetted(scaledImage, meta);
    }
}
