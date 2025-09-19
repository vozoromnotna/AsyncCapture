using AsyncCapture.Core.Cameras.CameraProperties;
using Nito.AsyncEx;

namespace AsyncCapture.Iray.Properties;

public class IrayZoomProp : IntProperty
{
    public override int MinIncrement => 1;

    public override string Name => "Zoom";

    public override string DisplayName => "Увеличение";
    
    private int _minValue;
    private int _maxValue;
    public override int MinValue { get => _minValue; }
    public override int MaxValue { get => _maxValue; }


    public IrayZoomProp(IrayComAdapter comControl)
    {
        _comControl = comControl;
        _zoomValue = comControl.GetZoomPosition().Result;
        _minValue = _comControl.MinZoomPosition;
        _maxValue = _comControl.MaxZoomPosition;
    }

    private IrayComAdapter _comControl;

    int _zoomValue;
    public override int GetValue()
    {
        var res = _comControl.GetZoomPosition().Result;
        return res;
    }

    Task _setTask;
    public override void SetValue(int val)
    {
        var valToSet = val;
        if (_setTask == null || _setTask.IsCompleted)
        {
            _setTask = Task.Run(() =>
            {
                setZoom(valToSet);
                _zoomValue = valToSet;
                Task.Delay(50).Wait();
                Update();

            });
        }

    }
    private void setZoom(int val)
    {
        if (val == _zoomValue) return;
        if (_zoomValue > val)
        {
            _comControl.ZoomShort();
            while (true)
            {
                var curVal = _comControl.GetZoomPosition().Result;
                if (curVal < 0)
                {
                    Task.Delay(50).Wait();
                    continue;
                }
                if (curVal <= val) break;
            }
        }
        else
        {
            _comControl.ZoomLong();
            while (true)
            {
                var curVal = _comControl.GetZoomPosition().Result;
                if (curVal < 0)
                {
                    Task.Delay(50).Wait();
                    continue;
                }
                if (curVal >= val) break;
            }
        }
        _comControl.ZoomShutoff();
    }
}

public class IrayFocusProp : IntProperty
{
    public override int MinIncrement => 1;
    
    private int _minValue;
    private int _maxValue;
    public override int MinValue { get => _minValue; }
    public override int MaxValue { get => _maxValue; }

    public override string Name => "Focus";

    public override string DisplayName => "Фокус";


    public IrayFocusProp(IrayComAdapter comControl)
    {
        _comControl = comControl;
        _focusValue = _comControl.GetFocusPosition().Result;
        _maxValue = _comControl.MaxFocusPosition;
        _minValue = _comControl.MinFocusPosition;
    }

    private IrayComAdapter _comControl;

    int _focusValue;
    public override int GetValue()
    {
        var res = _comControl.GetFocusPosition().Result;
        return res;
    }

    Task _setTask;
    public override void SetValue(int val)
    {
        var valToSet = val;
        if (_setTask == null || _setTask.IsCompleted)
        {
            _setTask = Task.Run(() =>
            {
                setFocus(valToSet);
                _focusValue = valToSet;
                Task.Delay(50).Wait();
                Update();

            });
        }

    }
    private void setFocus(int val)
    {
        if (val == _focusValue) return;
        if (_focusValue < val)
        {
            _comControl.FocusNear();
            while (true)
            {
                var curVal = _comControl.GetFocusPosition().Result;
                if (curVal < 0)
                {
                    Task.Delay(50).Wait();
                    continue;
                }
                if (curVal >= val) break;
            }
        }
        else
        {
            _comControl.FocusFar();
            while (true)
            {
                var curVal = _comControl.GetFocusPosition().Result;
                if (curVal < 0)
                {
                    Task.Delay(50).Wait();
                    continue;
                }
                if (curVal <= val) break;
            }
        }
        _comControl.FocusShutoff();

    }

}



public class IrayPaletteProp : ListProperty
{
    IrayComAdapter _comControl;
    public override string Name => "Palette";

    public override string DisplayName => "Палитра";

    private Dictionary<string, byte[]> nameBytesPairs;
    public IrayPaletteProp(IrayComAdapter comControl)
    {
        _comControl = comControl;
        _values = new System.Collections.ObjectModel.ObservableCollection<string>
        {
            "Whitehot",
            "Blackhot",
            "Rainbow",
            "Rainbow HC",
            "Iron",
            "Lava",
            "Sky",
            "Mid-gray",
            "Gray-red",
            "Purple-orange",
            "Special",
            "Warining red",
            "Icefire",
            "Cyanred",
            "Special 2",
            "Gradient red",
            "Gradient geen",
            "Gradient blue",
            "Warning green",
            "Warning blue",
        };

        nameBytesPairs = new Dictionary<string, byte[]>
        {
            { "Whitehot", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x00, 0xF4, 0xEB, 0xAA } },
            { "Blackhot", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x01, 0xF5, 0xEB, 0xAA } },
            { "Rainbow", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x02, 0xF6, 0xEB, 0xAA } },
            { "Rainbow HC", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x03, 0xF7, 0xEB, 0xAA } },
            { "Iron", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x04, 0xF8, 0xEB, 0xAA } },
            { "Lava", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x05, 0xF9, 0xEB, 0xAA } },
            { "Sky", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x06, 0xFA, 0xEB, 0xAA } },
            { "Mid-gray", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x07, 0xFB, 0xEB, 0xAA } },
            { "Gray-red", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x08, 0xFC, 0xEB, 0xAA } },
            { "Purple-orange", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x09, 0xFD, 0xEB, 0xAA } },
            { "Special", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x0A, 0xFE, 0xEB, 0xAA } },
            { "Warining red", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x0B, 0xFF, 0xEB, 0xAA } },
            { "Icefire", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x0C, 0x00, 0xEB, 0xAA } },
            { "Cyanred", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x0D, 0x01, 0xEB, 0xAA } },
            { "Special 2", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x0E, 0x02, 0xEB, 0xAA } },
            { "Gradient red", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x0F, 0x03, 0xEB, 0xAA } },
            { "Gradient geen", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x10, 0x04, 0xEB, 0xAA } },
            { "Gradient blue", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x11, 0x05, 0xEB, 0xAA } },
            { "Warning green", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x12, 0x06, 0xEB, 0xAA } },
            { "Warning blue", new byte[]{ 0xAA, 0x05, 0x01, 0x42, 0x02, 0x13, 0x07, 0xEB, 0xAA } },
        };

        var bytes = new byte[] { 0xAA, 0x05, 0x01, 0x42, 0x00, 0x00, 0xF2, 0xEB, 0xAA };

        var res = AsyncContext.Run(async () => await _comControl.SendMessage(bytes));
        try
        {
            var curPalette = res[4];
            _selectedIndex = curPalette;
        }
        catch
        {
            _selectedIndex = 0;
        }
    }

    protected override async void SelectedItemChanged()
    {
        var bytes = nameBytesPairs[_values[SelectedIndex]];
        var res = await _comControl.SendMessage(bytes);
        if (res[4] != 0x01)
            throw new Exception("ошибка в установке палитры");
    }
}


