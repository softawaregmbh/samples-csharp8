using System;
using System.Collections.Generic;
using System.Text;

namespace Samples
{
    class NullDataSource : IDataSource
    {
        public IEnumerable<string> GetData()
        {
            yield break;
        }
    }
}
