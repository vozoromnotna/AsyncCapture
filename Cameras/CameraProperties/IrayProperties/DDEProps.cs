using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraProperties.IrayProperties
{
    public partial class IrayGainControl
    {
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
