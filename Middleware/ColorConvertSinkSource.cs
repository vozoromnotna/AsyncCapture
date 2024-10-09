using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Middleware;

public class ColorConvertSinkSource : MatSource, ISinkSource<Mat>
{
    private readonly ColorConversionCodes _colorConversionCode;
    public ColorConvertSinkSource(ColorConversionCodes colorConversionCode) 
    {
        _colorConversionCode = colorConversionCode;
    }
    public async Task PutImage(Mat image, Dictionary<string, object> meta)
    {
        Cv2.CvtColor(image, image, _colorConversionCode);
        await imageGetted(image, meta);
    }
}
