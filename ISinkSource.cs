using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture
{
    public interface ISinkSource<T> : ISink<T>, ISource<T>
    {

    }
}
