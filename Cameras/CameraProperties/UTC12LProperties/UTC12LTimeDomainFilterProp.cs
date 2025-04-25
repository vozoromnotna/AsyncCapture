using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraProperties.UTC12LProperties
{
    public class UTC12LTimeDomainFilterProp : BoolProperty
    {
        public override string Name => "Time_Domain_Filter";

        public override string DisplayName => "Time Domain Filter";

        UTC12LComAdapter _comController;
        public UTC12LTimeDomainFilterProp(UTC12LComAdapter comController)
        {
            _comController = comController;
            Task.Delay(0).ContinueWith(t => Value = false);
        }

        public override async void SetValue(bool val)
        {
            await _comController.TimeDomainFilter(val);
        }
    }
}
