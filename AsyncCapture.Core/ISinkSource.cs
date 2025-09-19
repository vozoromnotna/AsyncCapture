using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Core
{
    public interface ISinkSource<T> : ISink<T>, ISource<T>
    {

    }
}
