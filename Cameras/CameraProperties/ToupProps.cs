
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace AsyncCapture.Cameras.CameraProperties;

public sealed class ToupAutoExp : BoolProperty
{
    private Tcam nncam;

    public override string Name => "Auto_Exposure";

    public override string DisplayName => "Авто. экспозиция";

    ToupExp _toupExp;
    public ToupAutoExp(Tcam nncam, ToupExp toupExp)
    {
        this.nncam = nncam;
        _toupExp = toupExp;
        _value = GetValue();
        toupExp.IsEnabled = !_value;
    }

    public override bool GetValue()
    {
        nncam.get_AutoExpoEnable(out bool curValue);
        return curValue;
    }

    public override void SetValue(bool val)
    {
        var res = nncam?.put_AutoExpoEnable(val);
        if (val)
        {
            _toupExp.IsEnabled = false;
        }
        else
        {
            _toupExp.IsEnabled = true;
        }
    }
}

public sealed class ToupExp : uIntProperty
{
    private Tcam nncam;
    public override string Name => "Exposure";

    public override uint MinIncrement => 1;

    public override string DisplayName => "Экспозиция, мкс";

    public ToupExp(Tcam cam)
    {
        nncam = cam;
        uint nDef = 0;
        nncam?.get_ExpTimeRange(out _minValue, out _maxValue, out nDef);
        nncam?.get_ExpoTime(out _value);
    }
    public override uint GetValue()
    {
        nncam.get_ExpoTime(out uint curValue);
        return curValue;
    }
    public override void SetValue(uint val)
    {
        nncam?.put_ExpoTime(val);
    }

}

public class ToupResolution : ListProperty
{
    private Tcam nncam;
    private Tcam.DelegateEventCallback delegateEventCallback;
    private Action<WriteableBitmap> action;
    public ToupResolution(Tcam cam, Tcam.DelegateEventCallback delegateEventCallback, Action<WriteableBitmap> action) : base()
    {
        this.nncam = cam;
        uint resnum = nncam.ResolutionNumber;
        for (uint i = 0; i < resnum; ++i)
        {
            int w = 0, h = 0;
            if (nncam.get_Resolution(i, out w, out h))
                values.Add(w.ToString() + "*" + h.ToString());
        }
        uint u_selectedIndex = 0;
        nncam.get_eSize(out u_selectedIndex);
        selectedIndex = Convert.ToInt32(u_selectedIndex);
        this.delegateEventCallback = delegateEventCallback;
        this.action = action;
    }

    public override string Name => "Resolution";

    public override string DisplayName => "Разрешение";

    protected override void SelectedItemChanged()
    {
        base.SelectedItemChanged();
        nncam.Stop();

        nncam.put_eSize(Convert.ToUInt32(selectedIndex));

        int width = 0, height = 0;
        nncam.get_Size(out width, out height);
        var bitmap = new WriteableBitmap(width, height, 0, 0, PixelFormats.Bgr32, null);
        action(bitmap);
        nncam.StartPullModeWithCallback(delegateEventCallback);
    }
}

public class ToupFilterResolution : ListProperty
{
    private Tcam nncam;
    internal ToupFilterResolution(Tcam cam) : base()
    {
        this.nncam = cam;
        uint resnum = nncam.ResolutionNumber;
        for (uint i = 0; i < resnum; ++i)
        {
            int w = 0, h = 0;
            if (nncam.get_Resolution(i, out w, out h))
                values.Add(w.ToString() + "*" + h.ToString());
        }
        uint u_selectedIndex = 0;
        nncam.get_eSize(out u_selectedIndex);
        selectedIndex = Convert.ToInt32(u_selectedIndex);

    }

    public override string Name => "Resolution";

    public override string DisplayName => "Разрешение";

    protected override void SelectedItemChanged()
    {
        base.SelectedItemChanged();
        nncam.Stop();

        nncam.put_eSize(Convert.ToUInt32(selectedIndex));

        int width = 0, height = 0;
        nncam.get_Size(out width, out height);
        //toupCam.NewBMP(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
    }
}
