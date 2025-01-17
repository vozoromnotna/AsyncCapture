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

        UTC12LComController _comController;
        public UTC12LTimeDomainFilterProp(UTC12LComController comController)
        {
            _comController = comController;
            Value = false;
        }

        public override void SetValue(bool val)
        {
            _comController.TimeDomainFilter(val);
        }
    }
}
