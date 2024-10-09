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

namespace AsyncCapture.Cameras.CameraSources.Toup;


public sealed class ToupCS : CameraSource
{

    public ToupCS(Tcam nncam)
    {
        _nncam = nncam;
        startDevice(nncam);
    }


    private Tcam _nncam = null;
    private Bitmap _bmpToFiter = null;
    private bool _triggerMode = false;

    public override string Name => "SWIR";
    public bool TriggerMode
    {
        get => _triggerMode; set => _triggerMode = value;
    }

    private bool _isLive = true;
    public override bool IsLive => _isLive;

    private void startDevice(Tcam nncam)
    {
        if (nncam != null)
        {
            nncam.put_Option(eOPTION.OPTION_BITDEPTH, 0);
            nncam.put_Option(eOPTION.OPTION_RGB, 2); // RGB32

            uint resnum = _nncam.ResolutionNumber;
            uint eSize = 0;
            if (_nncam.get_eSize(out eSize))
            {
                int width = 0, height = 0;
                if (nncam.get_Size(out width, out height))
                {
                    /* The backend of WPF/UWP/WinUI is Direct3D/Direct2D, which is different from Winform's backend GDI.
                     * We use their respective native formats, Bgr32 in WPF/UWP/WinUI, and Bgr24 in Winform
                     */
                    _bmpToFiter = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    if (!nncam.StartPullModeWithCallback(new DelegateEventCallback(DelegateOnEventCallback)))
                        throw new Exception("Failed to start camera");
                }
            }
            nncam.put_VFlip(true);
            nncam.put_HFlip(false);

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

        if (_bmpToFiter != null)
        {
            try
            {
                BitmapData bmpdata = _bmpToFiter.LockBits(new System.Drawing.Rectangle(0, 0, _bmpToFiter.Width, _bmpToFiter.Height), ImageLockMode.WriteOnly, _bmpToFiter.PixelFormat);
                try
                {
                    var bOK = _nncam.PullImageV3(bmpdata.Scan0, 0, 24, bmpdata.Stride, out var info); // check the return value
                }
                finally
                {
                    _bmpToFiter.UnlockBits(bmpdata);

                    var meta = new Dictionary<string, object>();

                    if (_isLive)
                        await imageGetted(_bmpToFiter.ToMat(), meta);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

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
