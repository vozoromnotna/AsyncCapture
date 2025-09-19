using AsyncCapture.Core.Cameras.CameraProperties;
using AsyncCapture.Iray.Properties;

namespace AsyncCapture.Wpf.Iray;

public class IrayAutoFocus : ButtonProperty
{
    public override string Name => "Auto_Focus";

    public override string DisplayName => "Авто. фокус";

    public override void OnButtonClicked()
    {
        _comControl.AutoFocus();
        Task.Run(async () =>
        {
            _focusProp.IsEnabled = false;
            await Task.Delay(5000);
            _focusProp.IsEnabled = true;
            _focusProp.Update();
        });
    }

    private IrayFocusProp _focusProp;
    private IrayComAdapter _comControl;
    public IrayAutoFocus(IrayComAdapter comControl, IrayFocusProp focusProp)
    {
        _comControl = comControl;
        _focusProp = focusProp;
    }
}