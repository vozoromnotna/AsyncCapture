using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraProperties.IrayProperties
{
    public enum IrayImageMode
    {
        Manual = 0,
        Auto0,
        Auto1,
    }
    public partial class IrayGainControl
    {
        IrayComAdapter _comControl;

        IrayImageMode _imageMode = IrayImageMode.Auto0;
        public IrayImageMode ImageMode { get => _imageMode; }

        public IrayGainControl(IrayComAdapter comControl)
        {
            _comControl = comControl;
        }

        public event EventHandler ImageModeChanged;


        public async Task<bool> SetImageMode(IrayImageMode mode)
        {
            byte modeCode = 0;
            byte crcCode = 0;
            switch (mode)
            {
                case IrayImageMode.Manual:
                    modeCode = 0;
                    crcCode = 0xD0;
                    break;
                case IrayImageMode.Auto0:
                    modeCode = 1;
                    crcCode = 0xD1;
                    break;
                case IrayImageMode.Auto1:
                    modeCode = 2;
                    crcCode = 0xD2;
                    break;
            }

            byte[] message = { 0xAA, 0x05, 0x01, 0x1F, 0x01, modeCode, crcCode, 0xEB, 0xAA };

            var res = await _comControl.SendMessage(message);

            if ((res == null) || (res.Length != 8) || (res[4] != 1))
                return false;

            _imageMode = mode;

            ImageModeChanged?.Invoke(this, null);

            return true;
        }

        public async Task SetContrast(byte contrast)
        {
            var message = _comControl.CreateBuffer(new byte[] { 0x01, 0x22, 0x01, contrast });
            await _comControl.SendMessage(message);
        }

        public async Task SetBrightness(ushort brightness)
        {
            var data = new byte[] { 0x01, 0x23, 0x01, 0, 0 };
            BitConverter.GetBytes(brightness).CopyTo(data, 3);
            var message = _comControl.CreateBuffer(data);
            await _comControl.SendMessage(message);
        }

    }

    public class IrayImageModeProp : ListProperty
    {
        public override string Name => "Gain_Mode";

        public override string DisplayName => "Режим усиления";

        IrayGainControl _gainControl;
        public IrayImageModeProp(IrayGainControl gainControl)
        {
            _gainControl = gainControl;
            values = new System.Collections.ObjectModel.ObservableCollection<string>
            {
                "Ручной",
                "Авто 0",
                "Авто 1",
            };

            AsyncContext.Run(async ()=> await _gainControl.SetImageMode(IrayImageMode.Auto0));
            selectedIndex = 1;
        }

        protected override async void SelectedItemChanged()
        {
            var res = await _gainControl.SetImageMode((IrayImageMode)selectedIndex);

        }
    }

    public class IrayContrastProp : IntProperty
    {
        public override int MinIncrement => 1;

        public override string Name => "Iray_Contrast";

        public override string DisplayName => "Контраст";

        IrayGainControl _gainControl;
        public IrayContrastProp(IrayGainControl gainControl)
        {
            _minValue = 0;
            _maxValue = 255;
            _value = 0;

            _gainControl = gainControl;

            IsEnabled = _gainControl.ImageMode == IrayImageMode.Manual;

            _gainControl.ImageModeChanged += (sender, e) => { IsEnabled = _gainControl.ImageMode == IrayImageMode.Manual; };
        }

        public override void SetValue(int val)
        {
            if (val < _minValue)
                val = _minValue;

            if (val > _maxValue)
                val = _maxValue;

            _gainControl.SetContrast((byte)val)
                .ContinueWith((x) => OnPropertyChanged(nameof(Value)));

        }


    }

    public class IrayBrightnessProp : IntProperty
    {
        public override int MinIncrement => 1;

        public override string Name => "Iray_Brightness";

        public override string DisplayName => "Яркость";

        IrayGainControl _gainControl;
        public IrayBrightnessProp(IrayGainControl gainControl)
        {
            _minValue = 0;
            _maxValue = ushort.MaxValue;
            _value = 0;

            _gainControl = gainControl;

            IsEnabled = _gainControl.ImageMode == IrayImageMode.Manual;

            _gainControl.ImageModeChanged += (sender, e) => { IsEnabled = _gainControl.ImageMode == IrayImageMode.Manual; };
        }

        public override void SetValue(int val)
        {
            if (val < _minValue)
                val = _minValue;

            if (val > _maxValue)
                val = _maxValue;

            _gainControl.SetBrightness((ushort)val)
                .ContinueWith((x) => OnPropertyChanged(nameof(Value)));
        }
    }

}
