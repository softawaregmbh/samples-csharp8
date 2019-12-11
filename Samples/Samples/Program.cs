using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;

namespace Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var dataSources = GetDataSources();

            for (int i = 0; i < dataSources.Length; i++)
            {
                var dataSource = dataSources[i];

                EnsureDataSourceIsNotNull(ref dataSource);

                await PrintItemsAsync(i, dataSource);
                Console.WriteLine();
            }
        }

        private static void EnsureDataSourceIsNotNull([NotNull]ref IDataSource? dataSource)
        {
            dataSource ??= new NullDataSource();
        }

        private static IDataSource?[] GetDataSources()
        {
            return new IDataSource?[]
            {
                new InMemoryDataSource(new[]
                {
                    "In", "Memory", "Data", "Source"
                }),
                null,
                new HttpDataSource(new HttpClient(), "https://raw.githubusercontent.com/softawaregmbh/samples-csharp8/master/http.json"),
                new FileDataSource("file.txt")
            };
        }

        private static async Task PrintItemsAsync(int index, IDataSource dataSource)
        {
            Console.WriteLine($"DataSource {index}: {dataSource.GetType().Name}");

            switch (dataSource)
            {
                case IAsyncEnumerableDataSource aeds:
                    await foreach (var item in aeds.GetDataAsyncEnumerable())
                    {
                        Console.WriteLine(item);
                    }

                    break;

                case IAsyncDataSource ads:
                    foreach (var item in await ads.GetDataAsync())
                    {
                        Console.WriteLine(item);
                    }

                    break;

                default:
                    foreach (var item in dataSource.GetData())
                    {
                        Console.WriteLine(item);
                    }

                    break;
            }
        }
    }
}
