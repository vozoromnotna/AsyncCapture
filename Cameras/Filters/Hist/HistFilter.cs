
using AsyncCapture.Cameras.CameraProperties;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Point = System.Drawing.Point;
using Window = System.Windows.Window;

namespace AsyncCapture.Cameras.Filters.Hist;

public sealed class HistFilter : Filter
{
    public HistFilter()
    {
        properties.Add(new HistAutoProperty(this));
        properties.Add(new HistThresholdHighProperty(this));
        properties.Add(new HistThresholdLowProperty(this));
        properties.Add(new HistProperty(this));
        curvePoints = new ObservablePointCollection
        {
            new ObservablePoint(0, 0),
            new ObservablePoint(128, 128),
            new ObservablePoint(255, 255)
        };
        //curvePoints.CollectionChanged += CurvePoints_CollectionChanged;
        curvePoints.PointChanged += CurvePoints_PointChanged;
        curve = InterpolatePoints();
        
    }
    private int _histUpdateRate = 10;
    private int framecount = 0;
    protected override Mat _FilterImage(Mat inputImage)
    {

        if (inputImage.Channels() != 1)
        {
            Cv2.CvtColor(inputImage, inputImage, ColorConversionCodes.BGR2GRAY);
        }

        CalculateHist(inputImage);
        
        if (IsAuto)
        {
            SetCurveByImage(inputImage);
        }

        TransformImage(inputImage, inputImage.Width, inputImage.Height);

        framecount++;

        if (framecount >= _histUpdateRate)
        {
            framecount = 0;
        }

        // Возвращаем выходное изображение
        return inputImage;
    }
    
    bool _isAuto = true;
    public bool IsAuto
    {
        get => _isAuto;
        set
        {
            _isAuto = value;
            OnPropertyChanged();
        }
            
    }

    private double threshold = 0.05;
    public double Threshold
    {
        get => threshold;
        set => threshold = value;
    }

    private double thresholdLow = 0.05;
    public double ThresholdLow
    {
        get => thresholdLow;
        set => thresholdLow = value;
    }

    private double thresholdHigh = 0.95;
    public double ThresholdHigh
    {
        get => thresholdHigh;
        set => thresholdHigh = value;
    }

    private void CurvePoints_PointChanged(object sender, PointChangedEventArgs e)
    {

        var editedpoint = (sender as ObservablePoint);

        // Проверка на нахождение в границах от 0 до 255
        if (e.NewValue.X > 255)
        {
            editedpoint._x = 255;
            return;
        }
        if (e.NewValue.Y > 255)
        {
            editedpoint._y = 255;
            return;
        }

        if (e.NewValue.X < 0)
        {
            editedpoint._x = 0;
            return;
        }
        if (e.NewValue.Y < 0)
        {
            editedpoint._y = 0;
            return;
        }

        if (editedpoint != null)
        {
            var index = curvePoints.IndexOf(editedpoint);
            if (index == -1) return;
            // Проверяем, что точка всегда лежала между двумя соседними (проверка на монотонность)
            for (int i = 0; i < curvePoints.Count; i++)
            {
                if (index == i)
                    continue;

                if (index > i)
                {
                    if (e.NewValue.X <= curvePoints[i].X)
                    {
                        editedpoint._x = e.OldValue.X;
                        return;
                    }
                    if (e.NewValue.Y <= curvePoints[i].Y)
                    {
                        editedpoint._y = e.OldValue.Y;
                        return;
                    }
                }

                if (index < i)
                {
                    if (e.NewValue.X >= curvePoints[i].X)
                    {
                        editedpoint._x = e.OldValue.X;
                        return;
                    }
                    if (e.NewValue.Y >= curvePoints[i].Y)
                    {
                        editedpoint._y = e.OldValue.Y;
                        return;
                    }
                }
            }
        }
        UpdateCurve();
    }

    public override string Name => "Histogram";

    public override string DisplayName => "Коррекция гистограммы";

    public event Action OnHistChanged;
    public event Action OnCurveChanged;

    private ObservablePointCollection curvePoints;
    public ObservablePointCollection CurvePoints
    {
        get => curvePoints;
        set
        {
            curvePoints = value;
            UpdateCurve();
        }
    }

    private void UpdateCurve()
    {
        Curve = InterpolatePoints();
    }

    private Mat InterpolatePoints()
    {
        var points = curvePoints.Select(x => (double)x.X).ToList();
        var values = curvePoints.Select(x => (double)x.Y).ToList();
        var minValues = values.Min();
        var maxValues = values.Max();
        var newCurve = new Mat(1, 256, MatType.CV_8UC1);
        var data = new byte[256];
        IInterpolation interpol;
        interpol = Interpolate.Linear(points, values);

        for (short i = 0; i < curvePoints.First().X; i++)
        {
            data[i] = (byte)minValues;
        }
        for (short i = (byte)curvePoints.First().X; i < curvePoints.Last().X; i++)
        {
            var val = interpol.Interpolate(i);
            if (val < 0) val = 0;
            if (val > 255) val = 255;
            data[i] = Convert.ToByte(val);
        }
        for (short i = (byte)curvePoints.Last().X; i < 256; i++)
        {
            data[i] = (byte)maxValues;
        }
        newCurve.SetArray(data);
        return newCurve;
    }

    Mat curve;

    public Mat Curve
    {
        get => curve;
        set
        {
            curve = value;
            OnPropertyChanged();
            OnCurveChanged?.Invoke();
        }
    }

    double[] histValues;

    public double[] HistValues
    {
        get => histValues; 
        private set
        {
            histValues = value;
            OnPropertyChanged();
            OnHistChanged?.Invoke();
        }
    }


    private void CalculateHist(Mat matImage)
    {
        //int[] histogramSize = new int[] { 256 }; // Размер гистограммы
        //RangeF[] ranges = new RangeF[] { new RangeF(0, 256) }; // Диапазон значений яркости
        //DenseHistogram histogram = new DenseHistogram(histogramSize, ranges);
        
        //// Вычисление гистограммы
        //histogram.Calculate(new Image<Gray, byte>[] { matImage.ToImage<Gray, byte>() }, false, null);
        //Cv2.CalcHist()
        //var bins = histogram.GetBinValues();
        //var maxBin = bins.Max();
        //histValues = bins.Select(x => (double)255 * (x / maxBin)).ToArray();
        //OnHistChanged?.Invoke();
    }

    public void SetCurveByImage(Mat image)
    {
        CurvePoints.PointChanged -= CurvePoints_PointChanged;

        var thresholdLow = this.thresholdLow;
        var thresholdHigh = this.thresholdHigh;

        var histSum = histValues.Sum();
        var highPass = histSum* thresholdLow;
        var lowPass = histSum*thresholdHigh;

        
        int minValue = 0;
        int maxValue = 255;

        double curSum = 0;
        for (int i = 0; i < histValues.Length; i++)
        {
            curSum += histValues[i];
            if (curSum > highPass)
            {
                minValue = i;
                break;
            }                    
        }

        curSum = histSum;
        for(int i = histValues.Length - 1; i >= 0; i--)
        {
            curSum -= histValues[i];
            if (curSum < lowPass)
            {
                maxValue = i;
                break;
            }
        }

        //double[] minVal, maxVal;
        //Point[] minP, maxP;
        //image.MinMax(out minVal, out maxVal, out minP, out maxP);
        //var minValue = (byte)minVal[0];
        //var maxValue = (byte)maxVal[0];

        CurvePoints.First().SetXY(minValue, 0);

        CurvePoints[1].SetXY((int)((minValue + maxValue) / 2), 128);

        CurvePoints.Last().SetXY(maxValue, 255);
        CurvePoints.PointChanged += CurvePoints_PointChanged;
        UpdateCurve();

    }
    private void TransformImage(Mat image, int width, int height)
    {
        Cv2.LUT(image, Curve, image);

        //for (int y = 0; y < height; y++)
        //{
        //    for (int x = 0; x < width; x++)
        //    {
        //        byte pixelValue = image[y * width + x];
        //        byte newPixelValue = Curve[pixelValue];
        //        image[y * width + x] = newPixelValue;
        //    }
        //}
    }
}
public class HistProperty : ButtonProperty
{
    HistWindow _histWindow;
    HistFilter _filter;
    public override string Name => "Histogram";
    public override string DisplayName => "Гистограмма";
    public HistProperty(HistFilter filter)
    {
        _filter = filter;
    }

    public override void OnButtonClicked()
    {
        if (_histWindow != null)
        {
            if (_histWindow.IsVisible) return;
        }
        _histWindow = new HistWindow(_filter);
        _histWindow.Owner = Application.Current.MainWindow;
        
        _histWindow.Show();
    }
}

public class HistAutoProperty : BoolProperty
{
    HistFilter _filter;
    public HistAutoProperty(HistFilter filter)
    {
        _filter= filter;
    }

    private void _filter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _value = _filter.IsAuto;
    }

    public override string Name => "Auto";
    public override string DisplayName => "Авто.";

    public override bool GetValue()
    {
        return _filter.IsAuto;
    }
    public override void SetValue(bool val)
    {
        _filter.IsAuto = val;
    }

}

public class HistThresholdLowProperty : DoubleProperty
{
    HistFilter _filter;
    public HistThresholdLowProperty(HistFilter filter)
    {
        _filter = filter;
        _minValue = 0;
        _maxValue = 1;
    }

    public override string Name => "Lower_threshold";
    public override string DisplayName => "Нижний порог";

    public override double MinIncrement => 0.1;

    public override double GetValue()
    {
        return _filter.ThresholdLow;
    }

    public override void SetValue(double val)
    {
        _filter.ThresholdLow = val;
    }
}

public class HistThresholdHighProperty : DoubleProperty
{
    HistFilter _filter;
    public HistThresholdHighProperty(HistFilter filter)
    {
        _filter = filter;
        _minValue = 0;
        _maxValue = 1;
    }

    public override string Name => "Upper_threshold";
    public override string DisplayName => "Верхний порог";

    public override double MinIncrement => 0.1;

    public override double GetValue()
    {
        return _filter.ThresholdHigh;
    }

    public override void SetValue(double val)
    {
        _filter.ThresholdHigh = val;
    }


}

