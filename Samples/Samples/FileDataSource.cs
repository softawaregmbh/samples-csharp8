using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Samples
{
    class FileDataSource :
        IDataSource,
        IAsyncDataSource,
        IAsyncEnumerableDataSource
    {
        private readonly string path;

        public FileDataSource(string path)
        {
            this.path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public IEnumerable<string> GetData()
        {
            return File.ReadAllLines(this.path);
        }

        public async Task<IEnumerable<string>> GetDataAsync()
        {
            return await File.ReadAllLinesAsync(this.path);
        }

        public async IAsyncEnumerable<string> GetDataAsyncEnumerable()
        {
            using var reader = new StreamReader(this.path);

            while (true)
            {
                var line = await reader.ReadLineAsync();
                if (line == null)
                {
                    break;
                }

                yield return line;
            }
        }
    }
}
