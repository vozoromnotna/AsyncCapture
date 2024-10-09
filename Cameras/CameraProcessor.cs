using AsyncCapture;
using AsyncCapture.Cameras.Filters;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AsyncCapture.Cameras;

public class CameraProcessor : MatSource, ISinkSource<Mat>, INotifyPropertyChanged
{
    public CameraProcessor()
    {
        _filters = new ObservableCollection<Filter>();
        _filters.CollectionChanged += (s, e) =>
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    (e.NewItems[0] as Filter).SomePropertyChanged += Camera_OnPropertyChanged;
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    (e.OldItems[0] as Filter).SomePropertyChanged -= Camera_OnPropertyChanged;
                    break;
            }
        };


        _filters.Add(new TestFilter());

    }

    protected ObservableCollection<Filter> _filters;
    public ObservableCollection<Filter> Filters { get => _filters; }

    public event EventHandler SomePropertyChanged;
    private void Camera_OnPropertyChanged(object sender, EventArgs e)
    {
        SomePropertyChanged?.Invoke(sender, e);

    }

    public event PropertyChangedEventHandler PropertyChanged;
    

    public async Task PutImage(Mat image, Dictionary<string, object> meta)
    {
        foreach (var filter in _filters)
        {
            filter.FilterImage(image);
        }

        await imageGetted(image, meta);
    }

    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
    }

    
}
