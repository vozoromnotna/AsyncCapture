using AsyncCapture.Cameras.CameraProperties;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.Filters;

public sealed class GammaCorrectionFilter : Filter
{
    public override string Name => "Gamma_Corr";

    public override string DisplayName => "Гамма коррекция";

    public GammaCorrectionFilter()
    {
        properties.Add(new GammaFilterProp(this));
    }

    // constant in c*r^gamma where r is pixel value.
    private float gamma = 1;

    public double Gamma
    {
        get { return gamma; }
        set { gamma = (float)value; }
    }

    protected override Mat _FilterImage(Mat img)
    {
        //OpenCvSharp..IntensityTransform.IntensityTransformInvoke.GammaCorrection(img, img, gamma);
        return img;

    }
}
public class GammaFilterProp : DoubleProperty
{
    GammaCorrectionFilter filter;

    public override string Name => "Gamma";

    public override string DisplayName => "Гамма";

    public override double MinIncrement => 0.01;

    public GammaFilterProp(GammaCorrectionFilter filter)
    {
        this.filter = filter;
        this._maxValue = 2.2;
        this._minValue = 1 / 2.2;
    }

    public override void SetValue(double val)
    {
        filter.Gamma = val;
    }

    public override double GetValue()
    {
        return filter.Gamma;
    }
}
