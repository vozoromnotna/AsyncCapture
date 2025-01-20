using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraProperties.UTC12LProperties
{
    public class UTC12LWhiteHotProp : BoolProperty
    {
        public override string Name => "White_Hot";

        public override string DisplayName => "White Hot";

        private UTC12LComAdapter _comController;
        public UTC12LWhiteHotProp(UTC12LComAdapter comController)
        {
            _comController = comController;
            Value = true;
        }

        public override void SetValue(bool val)
        {
            _comController.WhiteHot(_value);
        }
    }
}
