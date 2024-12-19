using System.Net.Http;
using System.Threading.Tasks;

namespace BotInfrastructure.HttpClients
{
    public interface IHttpClient
    {
        Task<ResponseType> GetAsync<ResponseType>(string url);
        Task<ResponseType> PostAsync<ResponseType, RequestType>(string url, RequestType content);
        Task<ResponseType> PutAsync<ResponseType, RequestType>(string url, RequestType content);
        Task<ResponseType> DeleteAsync<ResponseType>(string url);
    }
}
