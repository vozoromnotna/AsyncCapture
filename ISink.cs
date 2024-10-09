using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture
{
    public interface ISink<T>
    {
        Task PutImage(T image, Dictionary<string, object> meta);
    }
}
