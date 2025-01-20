using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraProperties.UTC12LProperties
{
    public class UTC12LAutoFocusProp : ButtonProperty
    {
        public override string Name => "Auto_Focus";

        public override string DisplayName => "Авто фокус";

        UTC12LComAdapter _comController;
        public UTC12LAutoFocusProp(UTC12LComAdapter comController)
        {
            _comController = comController;
        }

        public override void OnButtonClicked()
        {
            IsEnabled = false;
            Task.Run(async () =>
            {
                await _comController.AutoFocus();
                await Task.Delay(10000);
                IsEnabled = true;
            });
        }
    }
}
