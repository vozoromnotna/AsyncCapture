using AsyncCapture.Core.Cameras.CameraProperties;

namespace AsyncCapture.Utc12.Properties
{
    public class UTC12LGainProp : IntProperty
    {
        public override int MinIncrement => 1;
        
        private int _minValue;
        private int _maxValue;
        public override int MinValue { get => _minValue; }
        public override int MaxValue { get => _maxValue; }

        public override string Name => "Gain";

        public override string DisplayName => "Усиление";

        private UTC12LComAdapter _controller;
        public UTC12LGainProp(UTC12LComAdapter controller) 
        {
            _minValue = 0;
            _maxValue = 255;
            _controller = controller;
            Task.Delay(0).ContinueWith((t) => Value = 128);
        }

        private int _valueToSet = 0;
        private bool _isBusy = false;
        private bool _isSet;
        public override async void SetValue(int val)
        {

            if (_isBusy)
            {
                _isSet = true;
                _valueToSet = val;
                return;
            }

            if ((val < 0) || (val > 255))
                return;

            //Trace.WriteLine("BUSY TRUE");
            _isSet = false;
            _isBusy = true;
            await _controller.SetGain((byte)val);

            //Trace.WriteLine("BUSY FALSE");
            _isBusy = false;
            if (_isSet)
            {
                SetValue(_valueToSet);
            }

        }
    }

    public class UTC12LBrightnessProp : IntProperty
    {
        public override int MinIncrement => 1;
        
        private int _minValue;
        private int _maxValue;
        public override int MinValue { get => _minValue; }
        public override int MaxValue { get => _maxValue; }

        public override string Name => "Brightness";

        public override string DisplayName => "Яркость";

        private UTC12LComAdapter _controller;
        public UTC12LBrightnessProp(UTC12LComAdapter controller)
        {
            _minValue = 0;
            _maxValue = 255;
            _controller = controller;

            Task.Delay(0).ContinueWith(t => Value = 128);
        }

        private int _valueToSet = 0;
        private bool _isBusy = false;
        private bool _isSet;
        public override async void SetValue(int val)
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

            await _controller.SetBrightness((byte)val);

            _isBusy = false;
            if (_isSet)
            {
                SetValue(_valueToSet);
            }

            
        }
    }
}
