using System;
using System.Collections.Generic;
using System.Text;

namespace Samples
{
    interface IDataSource
    {
        IEnumerable<string> GetData();
    }
}
