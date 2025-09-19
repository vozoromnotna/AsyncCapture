using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Core.Middleware;

public class FlipSinkSource : MatSource, ISinkSource<Mat>
{
    private readonly FlipMode _flipMode;
    public FlipSinkSource(FlipMode flipMode) 
    {
        _flipMode = flipMode;
    }
    public async Task PutImage(Mat image, Dictionary<string, object> meta)
    {
        Cv2.Flip(image, image, _flipMode);
        await imageGetted(image, meta);
    }

    public override void Stop()
    {
        throw new NotImplementedException();
    }
}

public class SimpleSinkSource : MatSource, ISinkSource<Mat>
{
    private readonly Action<Mat> _operation;
    public SimpleSinkSource(Action<Mat> operation) 
    {
        _operation = operation;
    }

    public async Task PutImage(Mat image, Dictionary<string, object> meta)
    {
        _operation(image);
        await imageGetted(image, meta);
    }

    public override void Stop()
    {
        throw new NotImplementedException();
    }
}
