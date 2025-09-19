using AsyncCapture.Core.Cameras.CameraProperties;

namespace AsyncCapture.Utc12.Properties
{
    public class UTC12LImageEnhancementProp : BoolProperty
    {
        public override string Name => "Image_Enhancement";

        public override string DisplayName => "Улучшение изображения";

        UTC12LComAdapter _comController;
        public UTC12LImageEnhancementProp(UTC12LComAdapter comController)
        {
            _comController = comController;
            Task.Delay(0).ContinueWith(t => Value = false);
            
        }

        public override async void SetValue(bool val)
        {
           await _comController.ImageEnhancment(val);
        }
    }
}
