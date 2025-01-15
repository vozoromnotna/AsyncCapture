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

        private int _valueToSet = 0;
        private bool _isBusy = false;
        private bool _isSet;
        public override void SetValue(int val)
        {
            if (_isBusy)
            {
                _isSet = true;
                _valueToSet = val;
                return;
            }

            if ((val < 0) || (val > 255))
                return;

            _isSet = false;
            _isBusy = true;
            _controller.SetGain((byte)val).ContinueWith((x) =>
            {
                _isBusy = false;
                if (_isSet)
                {
                    SetValue(_valueToSet);
                }

            });

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

        private int _valueToSet = 0;
        private bool _isBusy = false;
        private bool _isSet;
        public override void SetValue(int val)
        {
            if (_isBusy)
            {
                _isSet = true;
                _valueToSet = val;
                return;
            }

            if ((val < 0) || (val > 255))
                return;

            _isSet = false;
            _isBusy = true;
            _controller.SetBrightness((byte)val).ContinueWith((x) =>
            {
                _isBusy = false;
                if (_isSet)
                {
                    SetValue(_valueToSet);
                }

            });
            
        }
    }
}
