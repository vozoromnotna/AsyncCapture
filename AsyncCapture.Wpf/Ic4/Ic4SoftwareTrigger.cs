using AsyncCapture.Core.Cameras.CameraProperties;
using ic4;

namespace AsyncCapture.Wpf.Ic4;

public class Ic4SoftwareTrigger : ButtonProperty
{
    public override string Name => "Software_Trigger";

    public override string DisplayName => "Программный триггер";

    private PropCommand _softwareTrigger;
    public Ic4SoftwareTrigger(PropCommand softwareTrigger)
    {
        _softwareTrigger = softwareTrigger;
    }
    public override void OnButtonClicked()
    {
        _softwareTrigger.Execute();
    }
}