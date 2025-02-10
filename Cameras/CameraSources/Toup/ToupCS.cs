using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;
using System.Drawing.Imaging;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using System.Drawing;
using System.Security.Principal;
using OpenCvSharp.Extensions;
using static Tcam;
using AsyncCapture.Cameras.CameraProperties;
using OpenCvSharp;

namespace AsyncCapture.Cameras.CameraSources.Toup;


public sealed class ToupCS : CameraSource
{

    public ToupCS(Tcam nncam)
    {
        _nncam = nncam;
        startDevice(nncam);
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

    private void startDevice(Tcam nncam)
    {
        if (nncam != null)
        {
            nncam.put_Option(eOPTION.OPTION_BITDEPTH, 0);
            var res = nncam.put_Option(eOPTION.OPTION_RGB, 0); // RGB24
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

    private async void OnEventImageToFilter()
    {

        try
        {
            _nncam.get_ExpoTime(out var expoTime);
            _nncam.get_ExpoAGain(out var gain);
            var mat = new Mat(new OpenCvSharp.Size(_width, _height), MatType.CV_8UC3);
            try
            {
                var bOK = _nncam.PullImageV3(mat.Data, 0, 24, (int)mat.Step(), out var info);
            }
            finally
            {
                var meta = new Dictionary<string, object>
                {
                    { "exposition", (int)expoTime },
                    { "gain", (int)gain }
                };

                if (_isLive)
                    await imageGetted(mat, meta);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }


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
