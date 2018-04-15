using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EfficientApiCalls
{
    public class Program
    {

        private static readonly HttpClient client = new HttpClient(); 

        public static void Main(string[] args)
        {
            MainAsync(CancellationToken.None).Wait();
        }

        private static async Task MainAsync(CancellationToken cancellationToken)
        {
            // JIT
            //await BasicCallAsync();
            //await CancellableCallAsync(cancellationToken);
            //await CheckNetworkErrorCallAsync(cancellationToken);
            //await CustomExceptionCallAsync(cancellationToken);
            //await DeserializeFromStreamCallAsync(cancellationToken);
            var model = await DeserializeOptimizedFromStreamCallAsync(cancellationToken);

            Console.Clear();

            const int maxLoop = 100;

            await BenchmarkHelper.BenchAsync(BasicCallAsync, maxLoop, nameof(Program.BasicCallAsync));
            await BenchmarkHelper.BenchAsync(CancellableCallAsync, maxLoop, nameof(Program.CancellableCallAsync), cancellationToken);
            await BenchmarkHelper.BenchAsync(CheckNetworkErrorCallAsync, maxLoop, nameof(Program.CheckNetworkErrorCallAsync), cancellationToken);
            await BenchmarkHelper.BenchAsync(CustomExceptionCallAsync, maxLoop, nameof(Program.CustomExceptionCallAsync), cancellationToken);
            await BenchmarkHelper.BenchAsync(DeserializeFromStreamCallAsync, maxLoop, nameof(Program.DeserializeFromStreamCallAsync), cancellationToken);
            await BenchmarkHelper.BenchAsync(DeserializeOptimizedFromStreamCallAsync, maxLoop, nameof(Program.DeserializeOptimizedFromStreamCallAsync), cancellationToken);

            // POST
            await BenchmarkHelper.BenchAsync(PostBasicAsync, maxLoop, nameof(Program.PostBasicAsync), model, cancellationToken);
            await BenchmarkHelper.BenchAsync(PostStreamAsync, maxLoop, nameof(Program.PostStreamAsync), model, cancellationToken);
        }

        private const string Url =
            "http://localhost:5000/api/values";

        private static async Task<List<Model>> BasicCallAsync()
        {
            var content = await client.GetStringAsync(Url);
            return JsonConvert.DeserializeObject<List<Model>>(content);
        }

        private static async Task<List<Model>> CancellableCallAsync(CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, Url))
            using (var response = await client.SendAsync(request, cancellationToken))
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Model>>(content);
            }
        }

        private static async Task<List<Model>> CheckNetworkErrorCallAsync(CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, Url))
            using (var response = await client.SendAsync(request, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Model>>(content);
            }
        }

        public class ApiException : Exception
        {
            public int StatusCode { get; set; }

            public string Content { get; set; }
        }

        private static async Task<List<Model>> CustomExceptionCallAsync(CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, Url))
            using (var response = await client.SendAsync(request, cancellationToken))
            {
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode == false)
                    throw new ApiException { StatusCode = (int)response.StatusCode, Content = content };

                return JsonConvert.DeserializeObject<List<Model>>(content);
            }
        }

        private static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default(T);
            
            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var jr = new JsonSerializer();
                var searchResult = jr.Deserialize<T>(jtr);
                return searchResult;
            }
        }

        private static async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
            {
                using (var sr = new StreamReader(stream))
                {
                    content = await sr.ReadToEndAsync();
                }
            }
            
            return content;
        }

        private static async Task<List<Model>> DeserializeFromStreamCallAsync(CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, Url))
            using (var response = await client.SendAsync(request, cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                    return DeserializeJsonFromStream<List<Model>>(stream);

                var content = await StreamToStringAsync(stream);
                throw new ApiException { StatusCode = (int)response.StatusCode, Content = content };
            }
        }

        private static async Task<List<Model>> DeserializeOptimizedFromStreamCallAsync(CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, Url))
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                    return DeserializeJsonFromStream<List<Model>>(stream);

                var content = await StreamToStringAsync(stream);
                throw new ApiException { StatusCode = (int)response.StatusCode, Content = content };
            }
        }

        private static async Task PostBasicAsync(object content, CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, Url))
            {
                var json = JsonConvert.SerializeObject(content);
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    using (var response = await client
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                        .ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        }

        public static void SerializeJsonIntoStream(object value, Stream stream)
        {
            using (var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
            using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
            {
                var js = new JsonSerializer();
                js.Serialize(jtw, value);
                jtw.Flush();
            }
        }

        private static HttpContent CreateHttpContent(object content)
        {
            HttpContent httpContent = null;

            if (content != null)
            {
                var ms = new MemoryStream();
                SerializeJsonIntoStream(content, ms);
                ms.Seek(0, SeekOrigin.Begin);
                httpContent = new StreamContent(ms);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return httpContent;
        }

        private static async Task PostStreamAsync(object content, CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, Url))
            using (var httpContent = CreateHttpContent(content))
            {
                request.Content = httpContent;

                using (var response = await client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }
}
