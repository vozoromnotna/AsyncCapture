using AsyncCapture.Core.Cameras.CameraProperties;

namespace AsyncCapture.MultiSpectrum;

public class DistortionProperty : BoolProperty
{
    public override string Name => "Distortion_Removing";

    public override string DisplayName => "Компенсация дисторсии";

    private readonly EightEyeProcessor _processor;
    public DistortionProperty(EightEyeProcessor processor) { _processor = processor; }
    public override void SetValue(bool val)
    {
        _processor.DistortionRemove = val;
    }

    public override bool GetValue()
    {
        return _processor.DistortionRemove;
    }
}

public class VignettingProperty : BoolProperty
{
    public override string Name => "Vignetting_Removing";

    public override string DisplayName => "Компенсация виньетирования";

    private readonly EightEyeProcessor _processor;
    public VignettingProperty(EightEyeProcessor processor) { _processor = processor; }
    public override void SetValue(bool val)
    {
        _processor.VignettingRemove = val;
    }

    public override bool GetValue()
    {
        return _processor.VignettingRemove;
    }
}
