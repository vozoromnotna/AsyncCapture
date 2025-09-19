using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using ScottPlot;

namespace AsyncCapture.Wpf.Hist;

public class HistVM : INotifyPropertyChanged, IDisposable
{
    HistFilter _filter;
    WpfPlot plt;
    Dispatcher ui_dispatcher;
    public HistThresholdLowProperty ThresholdLowProperty { get; set; }
    public HistThresholdHighProperty ThresholdHighProperty { get; set; }
    public HistAutoProperty AutoProperty { get; set; }

    public HistVM(HistFilter filter, WpfPlot plt)
    {
        _filter = filter;
        this.plt = plt;


        ui_dispatcher = Dispatcher.CurrentDispatcher;
        filter.OnHistChanged += Filter_OnChanged;
        filter.OnCurveChanged += Filter_OnChanged;

        ThresholdLowProperty = _filter.Properties.First(x => x.GetType() == typeof(HistThresholdLowProperty)) as HistThresholdLowProperty;
        ThresholdHighProperty = _filter.Properties.First(x => x.GetType() == typeof(HistThresholdHighProperty)) as HistThresholdHighProperty;
        AutoProperty = _filter.Properties.First(x => x.GetType() == typeof(HistAutoProperty)) as HistAutoProperty;

        if (_filter.HistValues != null)
            if (_filter.Curve != null) 
                Filter_OnChanged();

        plt.MouseDown += Plt_MouseDown;
        
    }


    ObservablePoint _editedPoint;
    private void Plt_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (AutoProperty.Value) return;

        (double mouseCoordX, double mouseCoordY) = plt.GetMouseCoordinates();
        _editedPoint = GetPointNearest(mouseCoordX, mouseCoordY);
        if(_editedPoint != null)
        {
            plt.MouseDown -= Plt_MouseDown;
            plt.MouseMove += Plt_MouseMove;
            plt.MouseUp += Plt_MouseUp;
        }
    }

    private void Plt_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        (double mouseCoordX, double mouseCoordY) = plt.GetMouseCoordinates();
        if(!IsPointCorrect(mouseCoordX, mouseCoordY)) return;
        _editedPoint.SetXY((int)mouseCoordX, (int)mouseCoordY);
        
    }

    private bool IsPointCorrect(double x, double y)
    {
        if (x < 0 || x > 255) return false;
        if (y < 0 || y > 255) return false;
        return true;
    }

    private void Plt_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _editedPoint = null;
        plt.MouseMove -= Plt_MouseMove;
        plt.MouseUp -= Plt_MouseUp;
        plt.MouseDown += Plt_MouseDown;
    }

    private ObservablePoint GetPointNearest(double mouseCoordX, double mouseCoordY)
    {
        foreach (var point in  _filter.CurvePoints) 
        {
            var dist = Math.Abs(point.X - mouseCoordX) + Math.Abs(point.Y - mouseCoordY);
            if (dist < 10)
                return point;
        }
        return null;
    }

    private void Filter_OnChanged()
    {
        var xs = _filter.CurvePoints.Select(x => (double)x.X).ToArray();
        var ys = _filter.CurvePoints.Select(y => (double)y.Y).ToArray();
        _filter.Curve.GetArray<double>(out var ys_line);
        var xs_line = Enumerable.Range(0, 256).Select(x => (double)x).ToArray();
        ui_dispatcher.BeginInvoke(new Action(() =>
        {
            plt.Plot.Clear();

            plt.Plot.AddBar(_filter.HistValues);
            

            

            plt.Plot.AddScatterPoints(xs, ys, color: System.Drawing.Color.Red, markerSize: 10);


            foreach (var x in xs) plt.Plot.AddVerticalLine(x, color: System.Drawing.Color.Gray, style:ScottPlot.LineStyle.Dash);

            

            plt.Plot.AddScatterLines(xs_line, ys_line, color: System.Drawing.Color.Black, lineWidth: 1);



            plt.Refresh();
        }));
        
    }
     


    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
    }

    public void Dispose()
    {

        _filter.OnHistChanged -= Filter_OnChanged;
        _filter.OnCurveChanged -= Filter_OnChanged;

        plt.MouseDown -= Plt_MouseDown;


    }
}
