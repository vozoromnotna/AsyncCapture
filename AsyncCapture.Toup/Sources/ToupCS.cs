using AsyncCapture.Core.Cameras;
using AsyncCapture.Toup.Properties;
using OpenCvSharp;
using static AsyncCapture.Toup.Sources.Tcam;

namespace AsyncCapture.Toup.Sources;


public sealed class ToupCS : CameraSource
{

    public ToupCS(Tcam nncam)
    {
        _nncam = nncam;
        startDeviceRaw(nncam);
    }


    private Tcam _nncam;

    private bool _triggerMode = false;

    public override string Name => "SWIR";
    public bool TriggerMode
    {
        get => _triggerMode; set => _triggerMode = value;
    }

    private bool _isLive = true;
    public override bool IsLive => _isLive;

    private int _width;
    private int _height;

    private bool _isRaw = false;
    private void startDevice(Tcam nncam)
    {
        if (nncam != null)
        {
            _isRaw = false;
            var res = nncam.put_Option(eOPTION.OPTION_RAW, 0);
            res = nncam.put_Option(eOPTION.OPTION_RGB, 0); // RGB24
            nncam.put_Option(eOPTION.OPTION_BITDEPTH, 0);
            uint resnum = _nncam.ResolutionNumber;
            uint eSize = 0;
            if (_nncam.get_eSize(out eSize))
            {

                if (nncam.get_Size(out _width, out _height))
                {
                    if (!nncam.StartPullModeWithCallback(new DelegateEventCallback(DelegateOnEventCallback)))
                        throw new Exception("Failed to start camera");
                }
            }
            nncam.put_VFlip(false);
            nncam.put_HFlip(true);

            nncam.get_RawFormat(out uint nFourCC, out uint bitdepth);
        }
    }

    private void startDeviceRaw(Tcam nncam)
    {
        if (nncam != null)
        {
            _isRaw = true;
            var res = nncam.put_Option(eOPTION.OPTION_RAW, 1);
            res = nncam.put_Option(eOPTION.OPTION_RGB, 4);
            res = nncam.put_Option(eOPTION.OPTION_BITDEPTH, 1);

            nncam.put_VFlip(true);
            nncam.put_HFlip(true);

            uint resnum = _nncam.ResolutionNumber;
            uint eSize = 0;
            if (_nncam.get_eSize(out eSize))
            {

                if (nncam.get_Size(out _width, out _height))
                {
                    if (!nncam.StartPullModeWithCallback(new DelegateEventCallback(DelegateOnEventCallback)))
                        throw new Exception("Failed to start camera");
                }
            }
            

            nncam.get_RawFormat(out uint nFourCC, out uint bitdepth);
        }
    }

    private void DelegateOnEventCallback(eEVENT evt)
    {

        if (_nncam != null)
        {
            switch (evt)
            {
                case eEVENT.EVENT_ERROR:

                    break;
                case eEVENT.EVENT_DISCONNECTED:

                    break;
                case eEVENT.EVENT_EXPOSURE:
                    OnEventExposure();
                    break;
                case eEVENT.EVENT_IMAGE:
                    //OnEventImage();
                    OnEventImageToFilter();

                    break;
                case eEVENT.EVENT_STILLIMAGE:

                    break;
                case eEVENT.EVENT_TEMPTINT:

                    break;
                default:
                    break;
            }
        }
        //}));
    }

    private void OnEventExposure()
    {
        var element = Properties.Where(x => x.Name == "Exposure").FirstOrDefault();
        if (element != null)
        {
            var exp_prop = element as ToupExp;
            exp_prop.Update();
        }

    }

    private async Task Get16Bit()
    {
        try
        {
            _nncam.get_ExpoTime(out var expoTime);
            _nncam.get_ExpoAGain(out var gain);
            var mat = new Mat(new OpenCvSharp.Size(_width, _height), MatType.CV_16UC1);
            var time = DateTime.Now;
            try
            {
                var bOK = _nncam.PullImageV3(mat.Data, 0, 16 , (int)mat.Step(), out var info);
            }
            finally
            {
                var meta = new Dictionary<string, object>
                {
                    { "time", time },
                    { "exposition", (int)expoTime },
                    { "gain", (int)gain }
                };

                if (_isLive)
                    await imageGetted(mat, meta);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task Get8Bit()
    {
        try
        {
            _nncam.get_ExpoTime(out var expoTime);
            _nncam.get_ExpoAGain(out var gain);
            var mat = new Mat(new OpenCvSharp.Size(_width, _height), MatType.CV_8UC3);
            var time = DateTime.Now;
            try
            {
                var bOK = _nncam.PullImageV3(mat.Data, 0, 24, (int)mat.Step(), out var info);
            }
            finally
            {
                var meta = new Dictionary<string, object>
                {
                    { "time", time },
                    { "exposition", (int)expoTime },
                    { "gain", (int)gain }
                };

                if (_isLive)
                    await imageGetted(mat, meta);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    private async void OnEventImageToFilter()
    {

        if (_isRaw)
            await Get16Bit();
        else
            await Get8Bit();


    }

    public override void Dispose()
    {
        _isLive = false;
        _nncam?.Stop();
        _nncam?.Close();
        _nncam?.Dispose();

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
