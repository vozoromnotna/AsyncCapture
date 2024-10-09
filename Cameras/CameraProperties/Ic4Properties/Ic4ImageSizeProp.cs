using ic4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraProperties.Ic4Properties;

public class Ic4ImageSizeController : INotifyPropertyChanged
{
    private PropInteger _width;
    private PropInteger _height;
    private PropInteger _binningH;
    private PropInteger _binningV;
    public Ic4ImageSizeController(PropInteger widthProp, PropInteger heightProp, PropInteger binningHorizontalProp = null, PropInteger binningVerticalProp = null)
    {
        _width = widthProp;
        _height = heightProp;
        _binningH = binningHorizontalProp;
        _binningV = binningVerticalProp;

        _width.Value = _width.Maximum;
        _height.Value = _height.Maximum;

        if (_binningH != null)
        {
            _binningH.Value = _binningH.Minimum;
        }

        if (_binningV != null)
        {
            _binningV.Value = _binningV.Minimum;
        }

    }

    public int Height
    {
        get => (int)_height.Value * BinningVertical;

        set
        {
            var steps = value / BinningVertical / _height.Increment;
            _height.Value = steps * _height.Increment;
            OnPropertyChanged();
        }
    }

    public (int, int) GetHeightRange()
    {
        return ((int)_height.Minimum, (int)_height.Maximum);
    }

    public int Width
    {
        get => (int)_width.Value * BinningHorizontal;
        set
        {
            var steps = value / BinningHorizontal / _width.Increment;
            _width.Value = steps * _width.Increment;
            OnPropertyChanged();
        }
    }

    public (int, int) GetWidthRange()
    {
        return ((int)_width.Minimum, (int)_width.Maximum);
    }
    public int BinningHorizontal
    {
        get => _binningH == null ? 1 : (int)_binningH.Value;
        set
        {
            if (_binningH == null)
                return;

            if (value > _binningH.Maximum)
            {
                _binningH.Value = _binningH.Maximum;
            }
            else if (value < _binningH.Minimum)
            {
                _binningH.Value = _binningH.Maximum;
            }
            else
            {
                _binningH.Value = value;
            }
            Height = Height;
            OnPropertyChanged();
        }
    }

    public (int, int) GetBinningHorizontalRange()
    {
        return (_binningH == null ? 1 : (int)_binningH.Minimum, _binningH == null ? 1 : (int)_binningH.Maximum);
    }
    public int BinningVertical
    {
        get => _binningV == null ? 1 : (int)_binningV.Value;
        set
        {
            if (_binningV == null)
                return;

            if (value > _binningV.Maximum)
            {
                _binningV.Value = _binningV.Maximum;
            }
            else if (value < _binningV.Minimum)
            {
                _binningV.Value = _binningV.Maximum;
            }
            else
            {
                _binningV.Value = value;
            }
            Width = Width;
            OnPropertyChanged();
        }
    }
    public (int, int) GetBinningVerticalRange()
    {
        return (_binningH == null ? 1 : (int)_binningH.Minimum, _binningH == null ? 1 : (int)_binningH.Maximum);
    }



    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
    }
}

public class Ic4HeightProp : IntProperty
{
    public override int MinIncrement => 1;

    public override string Name => "Image_heigth";

    public override string DisplayName => "Высота изображения, пкс";

    Ic4ImageSizeController _sizeController;
    public Ic4HeightProp(Ic4ImageSizeController sizeController) : base()
    {
        _sizeController = sizeController;
        (_minValue, _maxValue) = _sizeController.GetHeightRange();
    }

    public override int GetValue()
    {
        _value = _sizeController.Height;
        return base.GetValue();
    }

    public override void SetValue(int val)
    {
        _sizeController.Height = val;
    }
}

public class Ic4WidthProp : IntProperty
{
    public override int MinIncrement => 1;

    public override string Name => "Image_width";

    public override string DisplayName => "Ширина изображения, пкс";

    Ic4ImageSizeController _sizeController;
    public Ic4WidthProp(Ic4ImageSizeController sizeController) : base()
    {
        _sizeController = sizeController;
        (_minValue, _maxValue) = sizeController.GetWidthRange();
    }

    public override int GetValue()
    {
        _value = _sizeController.Width;
        return base.GetValue();
    }

    public override void SetValue(int val)
    {
        _sizeController.Width = val;
    }
}

public class Ic4BinningHorizontalProp : IntProperty
{
    public override int MinIncrement => 1;

    public override string Name => "Image_width";

    public override string DisplayName => "Горизонтальный биннинг";

    Ic4ImageSizeController _sizeController;
    public Ic4BinningHorizontalProp(Ic4ImageSizeController sizeController) : base()
    {
        _sizeController = sizeController;
        (_minValue, _maxValue) = _sizeController.GetBinningHorizontalRange();
    }

    public override int GetValue()
    {
        _value = _sizeController.BinningHorizontal;
        return base.GetValue();
    }

    public override void SetValue(int val)
    {
        _sizeController.BinningHorizontal = val;
    }
}

public class Ic4BinningVerticalProp : IntProperty
{
    public override int MinIncrement => 1;

    public override string Name => "Image_width";

    public override string DisplayName => "Вертикальный биннинг";

    Ic4ImageSizeController _sizeController;
    public Ic4BinningVerticalProp(Ic4ImageSizeController sizeController) : base()
    {
        _sizeController = sizeController;
        (_minValue, _maxValue) = _sizeController.GetBinningVerticalRange();
    }

    public override int GetValue()
    {
        _value = _sizeController.BinningHorizontal;
        return base.GetValue();
    }

    public override void SetValue(int val)
    {
        _sizeController.BinningHorizontal = val;
    }
}

public class Ic4ExposureProp : DoubleProperty
{
    public override double MinIncrement => throw new NotImplementedException();

    public override string Name => throw new NotImplementedException();

    public override string DisplayName => throw new NotImplementedException();


}
