using ic4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraProperties.Ic4Properties
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

}
