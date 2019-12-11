using System;
using System.Collections.Generic;

namespace Samples
{
    class InMemoryDataSource :
        IDataSource,
        IAsyncDataSource,          // default implementation
        IAsyncEnumerableDataSource // default implementation
    {
        private readonly IEnumerable<string> data;

        public InMemoryDataSource(IEnumerable<string> data)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public IEnumerable<string> GetData() => this.data;
    }
}
