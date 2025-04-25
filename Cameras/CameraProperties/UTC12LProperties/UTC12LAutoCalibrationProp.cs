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
            Task.Delay(0).ContinueWith(t => Value = true);
        }

        public override async void SetValue(bool val)
        {
            await _comController.AutoCalibration(val);
        }
    }
}
