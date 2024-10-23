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
    public class IrayGainControl
    {
        IrayCOMControl _comControl;

        IrayImageMode _imageMode = IrayImageMode.Auto0;
        public IrayImageMode ImageMode { get => _imageMode; }

        public IrayGainControl(IrayCOMControl comControl)
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

        public async void SetContrast(byte contrast)
        {
            var message = _comControl.CreateBuffer(new byte[] { 0x01, 0x22, 0x01, contrast });
            await _comControl.SendMessage(message);
        }

        public async void SetBrightness(ushort brightness)
        {
            var data = new byte[] { 0x01, 0x23, 0x01, 0, 0 };
            BitConverter.GetBytes(brightness).CopyTo(data, 3);
            var message = _comControl.CreateBuffer(data);
            await _comControl.SendMessage(message);
        }
        public async Task<bool> ReadDDEStatus()
        {
            var res = await _comControl.SendMessage(new byte[] { 0xAA, 0x05, 0x01, 0x1A, 0x00, 0x00, 0xCA, 0xEB, 0xAA });
            if (res == null)
                return false;

            if (res.Length != 8)
                return false;

            var status = res[4];

            return status == 0x01;
        }

        public async void SetDDEMode(byte mode)
        {
            if ((mode > 7) || (mode < 0))
                return;

            var message = _comControl.CreateBuffer(new byte[] { 0x01, 0x19, 0x01, (byte)(mode + 1), });
            var res = await _comControl.SendMessage(message);
        }

        public async void SetDDEStatus(bool status)
        {
            var message = _comControl.CreateBuffer(new byte[] { 0x01, 0x1A, 0x02, (byte)(status ? 0x01 : 0x00) });
            var res = await _comControl.SendMessage(message);
        }

        public async void SetImageFilterStatus(bool status)
        {
            var message = _comControl.CreateBuffer(new byte[] { 0x01, 0x1B, 0x02, (byte)(status ? 0x01 : 0x00) });
            await _comControl.SendMessage(message);
        }

        public async Task<bool> ReadImageFilterStatus()
        {
            var res = await _comControl.SendMessage(new byte[] { 0xAA, 0x05, 0x01, 0x1B, 0x00, 0x00, 0xCB, 0xEB, 0xAA });
            if (res == null)
                return false;

            if (res.Length != 8)
                return false;

            var status = res[4];

            return status == 0x01;
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

            _gainControl.SetContrast((byte)val);
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

            _gainControl.SetBrightness((ushort)val);
        }
    }

    public class IrayDDEModeProp : ListProperty
    {
        public override string Name => "DDE_Mode";

        public override string DisplayName => "DDE Mode";

        IrayGainControl _gainControl;
        public IrayDDEModeProp(IrayGainControl gainControl)
        {
            _gainControl = gainControl;
            values = new System.Collections.ObjectModel.ObservableCollection<string>();
            for (int i = 0; i < 8; i++)
                values.Add($"Mode {i}");

            _gainControl.SetDDEMode(0);
            selectedIndex = 0;
        }

        protected override void SelectedItemChanged()
        {
            _gainControl.SetDDEMode((byte)selectedIndex);
        }
    }

    public class IrayDDEStatusProp : BoolProperty
    {
        public override string Name => "DDE_On";

        public override string DisplayName => "DDE On";

        IrayGainControl _gainControl;
        public IrayDDEStatusProp(IrayGainControl gainControl)
        {
            _gainControl = gainControl;
            _value = AsyncContext.Run(() => gainControl.ReadDDEStatus());
        }

        public override void SetValue(bool val)
        {
            _gainControl.SetDDEStatus(val);
        }
    }

    public class IrayImageFilterStatusProp : BoolProperty
    {
        public override string Name => "Image_Filter_On";

        public override string DisplayName => "Image Filter On";

        IrayGainControl _gainControl;
        public IrayImageFilterStatusProp(IrayGainControl gainControl)
        {
            _gainControl = gainControl;
            _value = AsyncContext.Run(() => gainControl.ReadImageFilterStatus());
        }

        public override void SetValue(bool val)
        {
            _gainControl.SetImageFilterStatus(val);
        }
    }
}
