using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraProperties.UTC12LProperties
{
    public class UTC12LImageEnhancementProp : BoolProperty
    {
        public override string Name => "Image_Enhancement";

        public override string DisplayName => "Улучшение изображения";

        UTC12LComAdapter _comController;
        public UTC12LImageEnhancementProp(UTC12LComAdapter comController)
        {
            _comController = comController;
            Value = false;
        }

        public override void SetValue(bool val)
        {
            _comController.ImageEnhancment(val);
        }
    }
}
