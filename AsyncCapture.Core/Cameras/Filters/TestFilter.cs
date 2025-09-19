using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Core.Cameras.Filters;

public sealed class TestFilter : Filter
{
    public override string Name => "Test_Filter";

    public override string DisplayName => "Тестовый фильтр";

    protected override Mat _FilterImage(Mat img)
    {
        Cv2.ApplyColorMap(img, img, ColormapTypes.Jet);
        return img;
    }
}
