using AsyncCapture.Core.Cameras.CameraProperties;
using ic4;

namespace AsyncCapture.Ic4.Properties
{
    public class Ic4TriggerModeProp : BoolProperty
    {
        PropEnumeration _triggerModeProp;
        public Ic4TriggerModeProp(PropEnumeration triggerSourceProp, PropEnumeration triggerModeProp) 
        {
            triggerSourceProp.Value = "Any";
            _triggerModeProp = triggerModeProp;
        }

        public override void SetValue(bool val)
        {
            _triggerModeProp.Value = val ? "On" : "Off"; 
        }

        public override string Name => "Trigger_Mode";

        public override string DisplayName => "Изображение по триггеру";
    }



}
