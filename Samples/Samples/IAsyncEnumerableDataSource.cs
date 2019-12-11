using System;
using System.Collections.Generic;
using System.Text;

namespace Samples
{
    interface IAsyncEnumerableDataSource : IDataSource
    {
        public async IAsyncEnumerable<string> GetDataAsyncEnumerable()
        {
            var items = this switch
            {
                IAsyncDataSource asyncDataSource => await asyncDataSource.GetDataAsync(),
                _ => this.GetData()
            };

            foreach (var item in items)
            {
                yield return item;
            }
        }
    }
}
