using AsyncCapture.Core;
using OpenCvSharp;

namespace AsyncCapture.MultiSpectrum;

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

    private int[] _imageMap = new int[MaxImages];

    private Dictionary<ISource<Mat>, int> _channelIndexPairs = new Dictionary<ISource<Mat>, int>();

    private int _imageGetted = 0;
    private object _lock = new object();
    private TaskCompletionSource _tcs;

    public EightEyeConcatenator(int channels = MaxImages)
    {
        _tcs = new TaskCompletionSource();
        for (int i = 0; i < MaxImages; i++)
        {
            _imageMap[i] = i;
        }
    }

    
    public void SetMap(int[] map)
    {
        _imageMap = map;
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
        var output = Concat(_imageBuffer, IsBorders, BordersThickness, BordersColor);

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

    public bool IsBorders { get; set; } = false;
    public Scalar BordersColor { get; set; } = Scalar.Black;
    public int BordersThickness { get; set; } = 2;
    public Mat Concat(Mat[] mats, bool isBorders, int bordersThikness = 1, Scalar bordersColor = default(Scalar))
    {
        if (isBorders)
        {
            var borderedMats = new Mat[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                borderedMats[i] = new Mat();
                Cv2.CopyMakeBorder(
                    mats[i],
                    borderedMats[i],
                    bordersThikness,
                    bordersThikness,
                    bordersThikness,
                    bordersThikness,
                    BorderTypes.Constant,
                    bordersColor
                );
            }
            mats = borderedMats;
        }

        var output = new Mat();
        var upper = new Mat();
        var lower = new Mat();

        Mat[] reorderedMat = new Mat[mats.Length];
        for (int i = 0; i < reorderedMat.Length; i++)
        {
            var index = _imageMap[i];
            reorderedMat[i] = mats[index];

        }

        Cv2.HConcat(reorderedMat.Take(MaxImages / 2).ToArray(), upper);
        Cv2.HConcat(reorderedMat.Skip(MaxImages / 2).ToArray(), lower);
        Cv2.VConcat(new List<Mat> { upper, lower }, output);

        return output;
    }

    public override void Stop()
    {
        throw new NotImplementedException();
    }
}

