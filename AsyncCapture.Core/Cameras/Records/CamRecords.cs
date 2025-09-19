using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Core.Cameras.Records;

public class CamRecords
{
    public string Name { get; set; }
    
    public List<PropertyRecord> Properties { get; set; }
}
