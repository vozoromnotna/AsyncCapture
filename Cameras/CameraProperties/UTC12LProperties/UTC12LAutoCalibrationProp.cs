using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraProperties.UTC12LProperties
{
    public class UTC12LAutoCalibrationProp : BoolProperty
    {
        public override string Name => "Auto_Calibration";

        public override string DisplayName => "Авто калибровка";

        UTC12LComAdapter _comController;
        public UTC12LAutoCalibrationProp(UTC12LComAdapter comController)
        {
            _comController = comController;
            Value = true;
        }

        public override void SetValue(bool val)
        {
            _comController.AutoCalibration(val);
        }
    }
}
