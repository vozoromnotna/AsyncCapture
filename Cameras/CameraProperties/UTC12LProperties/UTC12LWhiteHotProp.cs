using Nito.AsyncEx;
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
            Task.Delay(0).ContinueWith(t => Value = true);
        }

        public override async void SetValue(bool val)
        {
            await _comController.WhiteHot(!_value);
        }
    }
}
