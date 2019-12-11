using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Samples
{
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
}
