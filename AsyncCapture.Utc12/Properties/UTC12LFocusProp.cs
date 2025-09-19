using AsyncCapture.Core.Cameras.CameraProperties;

namespace AsyncCapture.Utc12.Properties
{
    public class UTC12LFocusProp : ButtonPlusMinusProperty
    {
        public override string Name => "Focus";

        public override string DisplayName => "Фокус";

        UTC12LComAdapter _comController;
        public UTC12LFocusProp(UTC12LComAdapter comController)
        {
            _comController = comController;
        }

        public override void PlusMouseDown()
        {
            _comController.FocusFar();
        }

        public override void PlusMouseUp()
        {
            StopFocus();
        }


        public override void MinusMouseDown()
        {
            _comController.FocusNear();
        }
        public override void MinusMouseUp()
        {
            StopFocus();
        }

        private void StopFocus()
        {
            _comController.FocusStop();
        }
    }
}
