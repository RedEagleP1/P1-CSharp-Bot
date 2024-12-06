using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Bot_Infrastructure.HttpClients
{
    public class BotHttpClient : IHttpClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BotHttpClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateClient() => _httpClientFactory.CreateClient();

        public async Task<ResponseType> DeleteAsync<ResponseType>(string url)
        {
            var client = CreateClient();
            var response = await client.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ResponseType>(jsonString);
        }

        public async Task<ResponseType> GetAsync<ResponseType>(string url)
        {
            var client = CreateClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ResponseType>(jsonString);
        }

        public async Task<ResponseType> PostAsync<ResponseType, RequestType>(string url, RequestType content)
        {
            var client = CreateClient();
            var jsonContent = new StringContent(JsonSerializer.Serialize(content));
            var response = await client.PostAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ResponseType>(jsonString);
        }

        public async Task<ResponseType> PutAsync<ResponseType, RequestType>(string url, RequestType content)
        {
            var client = CreateClient();
            var jsonContent = new StringContent(JsonSerializer.Serialize(content));
            var response = await client.PutAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ResponseType>(jsonString);
        }
    }
}