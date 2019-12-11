using System.Collections.Generic;
using System.Threading.Tasks;

namespace Samples
{
    interface IAsyncDataSource : IDataSource
    {
        public Task<IEnumerable<string>> GetDataAsync()
        {
            return Task.Run(this.GetData);
        }
    }
}
