using System.Net.Http;
using System.Threading.Tasks;

namespace AutocompleteService.Services
{
    public class IndexApiService : IIndexApiService
    {
        private HttpClient _httpClient;

        public IndexApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> GetIndexesAsync(string word)
        {
            string APIURL = $"http://index:80/Index/autocomplete/{word}";
            var response = await _httpClient.GetAsync(APIURL);
            return response;
        }
    }

    public interface IIndexApiService
    {
        Task<HttpResponseMessage> GetIndexesAsync(string word);
    }
}
