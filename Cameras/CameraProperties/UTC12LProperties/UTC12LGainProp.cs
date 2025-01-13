using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraProperties.UTC12LProperties
{
    public class UTC12LGainProp : IntProperty
    {
        public override int MinIncrement => 1;

        public override string Name => "Gain";

        public override string DisplayName => "Усиление";

        private UTC12LComController _controller;
        public UTC12LGainProp(UTC12LComController controller) 
        {
            _minValue = 0;
            _maxValue = 255;
            _controller = controller;
        }

        public override void SetValue(int val)
        {
            if ((val < 0) || (val > 255))
                return;

            AsyncContext.Run(()=>_controller.SetGain((byte)val));
        }
    }

    public class UTC12LBrightnessProp : IntProperty
    {
        public override int MinIncrement => 1;

        public override string Name => "Brightness";

        public override string DisplayName => "Яркость";

        private UTC12LComController _controller;
        public UTC12LBrightnessProp(UTC12LComController controller)
        {
            _minValue = 0;
            _maxValue = 255;
            _controller = controller;
        }

        public override void SetValue(int val)
        {
            if ((val < 0) || (val > 255))
                return;

            AsyncContext.Run(async () => await _controller.SetBrightness((byte)val));
        }
    }
}
