# C# 8.0 samples

This sample project demonstrates some of the features introduced with C# 8.0, namely (in order of appearance):

* default interface members
* asynchronous streams
* switch expressions
* using declarations
* nullable reference types
* null-coalescing assignment

This repository contains the complete source code. You can jump right in or follow the walkthrough.

## Walkthrough

Start by creating an interface `IDataSource`:
```csharp
interface IDataSource
{
    IEnumerable<string> GetData();
}
```

In order to support asynchronous data access, add another interface:

```csharp
interface IAsyncDataSource : IDataSource
{
    public Task<IEnumerable<string>> GetDataAsync()
    {
        return Task.Run(this.GetData;
    }
}
```

The method `GetDataAsync` has a default implementation. Classes implementing `IAsyncDataSource` *can* choose to implement it, but they don't have to. If they don't, this default implementation will be used.

Let's add another derived interface that not only allows asynchronous data retrieval, but also asynchronous enumeration by using the new `IAsyncEnumerable<T>` type:

```csharp
interface IAsyncEnumerableDataSource : IDataSource
{
    IAsyncEnumerable<string> GetDataAsyncEnumerable();
}
```

We can also add a default implementation of this method. If `IAsyncDataSource` is implemented as well, we use its `GetDataAsync` method to retrieve the enumerable, otherwise we use `IDataSource`'s `GetData`:

```csharp
public async IAsyncEnumerable<string> GetDataAsyncEnumerable()
{
    IEnumerable<string> items;
    switch (this)
    {
        case IAsyncDataSource asyncDataSource:
            items = await asyncDataSource.GetDataAsync();
            break;
        default:
            items = this.GetData();
            break;
    }

    foreach (var item in items)
    {
        yield return item;
    }
}
```

It is now possible to use `await` and `yield` in the same method. Using the new `switch` expressions, we can simplify the code a bit:

```csharp
    var items = this switch
    {
        IAsyncDataSource asyncDataSource => await asyncDataSource.GetDataAsync(),
        _ => this.GetData()
    };
```

Let's add a simple implemenation. The only method we're required to implement is `GetData`. If we don't add the other ones, their default implementation will be used:

```csharp
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
```

If we want to, we can of course provide an implementation for selected...

```csharp
    class HttpDataSource :
        IDataSource,
        IAsyncDataSource,
        IAsyncEnumerableDataSource // default implementation
    {
        private readonly HttpClient httpClient;
        private readonly string url;

        public HttpDataSource(HttpClient httpClient, string url)
        {
            this.httpClient = httpClient;
            this.url = url;
        }

        public IEnumerable<string> GetData() => this.GetDataAsync().Result;

        public async Task<IEnumerable<string>> GetDataAsync()
        {
            var response = await this.httpClient.GetAsync(this.url);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<string>>(json);
        }
    }
```

...or all default interface methods with default implementations:

```csharp
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
```

Note that `using` doesn't have to be a block any more.

Next, we'll use all these types we created. Don't forget to add a `file.txt` content file as well and make sure it is copied to the output directory.

```csharp
class Program
{
    static async Task Main(string[] args)
    {
        var dataSources = GetDataSources();

        for (int i = 0; i < dataSources.Length; i++)
        {
            var dataSource = dataSources[i];
            await PrintItemsAsync(i, dataSource);
            Console.WriteLine();
        }
    }

    private static IDataSource[] GetDataSources()
    {
        return new IDataSource[]
        {
            new InMemoryDataSource(new[]
            {
                "In", "Memory", "Data", "Source"
            }),
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
```

`IAsyncEnumerable<T>` can be iterated by using `await foreach`.

Now let's try out the new nullable reference types. This feature must be enabled by added `<nullable>enable</nullable>` to the `<PropertyGroup>` in the `.csproj` file (or by using `#nullable enable` and `#nullable disable` directives).

Add a `null` entry to the array in `GetDataSources`.
Once we do that, the compiler produces warnings.

```csharp
        return new IDataSource[]
        {
            new InMemoryDataSource(new[]
            {
                "In", "Memory", "Data", "Source"
            }),
            null,
            new HttpDataSource(new HttpClient(), "https://raw.githubusercontent.com/softawaregmbh/samples-csharp8/master/http.json"),
            new FileDataSource("file.txt")
        };
```

The compiler shows a warning that we should change the type to `IDataSource?`.

```csharp
    private static IDataSource?[] GetDataSources()
    {
        return new IDataSource?[]
        ...
    }
```

Once we do that, we get a warning at the call to `PrintItemsAsync` that `dataSource` might be null.

We could get rid of the warning by adding `if (dataSource != null)` before the call, but that would mess up the indices in the output. Instead, let's implement the [null object pattern](https://en.wikipedia.org/wiki/Null_object_pattern):

```csharp
class NullDataSource : IDataSource
{
    public IEnumerable<string> GetData()
    {
        yield break;
    }
}
```

Add a method to `Program.cs` to make sure that there are no `null` data sources (using the new *null coalescing assignment* operator):

```csharp
    static async Task Main(string[] args)
    {
        var dataSources = GetDataSources();

        for (int i = 0; i < dataSources.Length; i++)
        {
            var dataSource = dataSources[i];
            EnsureDataSourceIsNotNull(ref dataSource);
            await PrintItemsAsync(i, dataSource);
        }            
    }

    private static void EnsureDataSourceIsNotNull(ref IDataSource? dataSource)
    {
        dataSource ??= new NullDataSource();
    }
```

This doesn't get rid of the warning though. *We* know that `dataSource` cannot be null after this method is called, but the compliler has no way of knowing, so we need to tell it by adding a `[NotNull]` attribute:

```csharp
    private static void EnsureDataSourceIsNotNull([NotNull] ref IDataSource? dataSource)
```

Here's a list of all the attributes for similar cases from the [docs](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-attributes#attributes-extend-type-annotations):

* `AllowNull`: A non-nullable input argument may be null
* `DisallowNull`: A nullable input argument should never be null
* `MaybeNull`: A non-nullable return value may be null
* `NotNull`: A nullable return value will never be null
* `MaybeNullWhen`: A non-nullable input argument may be null when the method returns the specified bool value
* `NotNullWhen`: A nullable input argument will not be null when the method returns the specified bool value
* `NotNullIfNotNull`: A return value isn't null if the argument for the specified parameter isn't null
