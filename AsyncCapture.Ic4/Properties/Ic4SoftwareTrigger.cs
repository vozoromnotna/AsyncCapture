using AsyncCapture.Core.Cameras.CameraProperties;
using AsyncCapture.Core.Cameras.Records;
using ic4;

namespace AsyncCapture.Ic4.Properties;

public class Ic4SoftwareTrigger : ButtonProperty
{
    public override string Name => "Software_Trigger";

    public override string DisplayName => "Программный триггер";
    public override void SetByPropertyRecord(PropertyRecord record)
    {
        throw new NotImplementedException();
    }

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