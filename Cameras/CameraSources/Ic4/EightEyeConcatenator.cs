using AsyncCapture;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraSources.Ic4;

public class EightEyeChannel : MatSource, ISinkSource<Mat>
{
    

    private readonly EightEyeConcatenator _concatenator;
    public EightEyeChannel(EightEyeConcatenator eightEyeConcatenator, int index)
    {
        _concatenator = eightEyeConcatenator;
        eightEyeConcatenator.SetChannel(this, index);
    }
    public async Task PutImage(Mat image, Dictionary<string, object> meta)
    {
        await _concatenator.PutImage(image, meta, this);
    }

    public override void Stop()
    {
        throw new NotImplementedException();
    }
}
public class EightEyeConcatenator : MatSource
{
    private const int MaxImages = 8;

    private Mat[] _imageBuffer = new Mat[MaxImages];

    private Dictionary<ISource<Mat>, int> _channelIndexPairs = new Dictionary<ISource<Mat>, int>();

    private int _imageGetted = 0;
    private object _lock = new object();
    private TaskCompletionSource _tcs;

    public EightEyeConcatenator(int channels = MaxImages)
    {
        _tcs = new TaskCompletionSource();
    }

    public void SetChannel(EightEyeChannel channel, int index)
    {
        _channelIndexPairs[channel] = index;
    }

    Dictionary<string, object> _meta;
    public async Task PutImage(Mat image, Dictionary<string, object> meta, EightEyeChannel sender)
    {
        var index = _channelIndexPairs[sender];
        _imageBuffer[index] = image;

        lock (_lock)
        {
            _imageGetted++;
        }

        if (_imageGetted == MaxImages)
        {
            await process();
            _tcs?.SetResult();

            _tcs = new TaskCompletionSource();
            
        }
        else
        {
            await WaitNextImageAsync();
        }
        
    }

    private async Task process()
    {
        var output = concat(_imageBuffer);

        await imageGetted(output, _meta);

        lock (_lock)
        {
            _imageGetted = 0;
        }
    }

    private async Task WaitNextImageAsync()
    {
        await _tcs.Task;
    }

    private Mat concat(Mat[] mats)
    {
        var output = new Mat();
        var upper = new Mat();
        var lower = new Mat();
        Cv2.HConcat(mats.Take(MaxImages / 2), upper);
        Cv2.HConcat(mats.Skip(MaxImages / 2), lower);
        Cv2.VConcat(new List<Mat> { upper, lower }, output);
        return output;
    }

    public override void Stop()
    {
        throw new NotImplementedException();
    }
}

