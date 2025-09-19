
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncCapture.Core.Cameras.Records;

namespace AsyncCapture.Core.Cameras.CameraProperties;

public interface IPropertyContainer
{
    string Name { get; }
    
    event EventHandler SomePropertyChanged;
    ObservableCollection<PropertyBase> Properties { get; }
    List<PropertyRecord> GetPropertyRecords();
}
