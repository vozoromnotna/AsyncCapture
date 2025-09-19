using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Core.Cameras.Records;

public class PropertyRecord : INotifyPropertyChanged
{
    private string _name;
    private string _value;
    public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
    public string Value { get => _value; set { _value = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
    }
}
