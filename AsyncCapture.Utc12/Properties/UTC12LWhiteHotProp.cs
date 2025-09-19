using AsyncCapture.Core.Cameras.CameraProperties;

namespace AsyncCapture.Utc12.Properties
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
