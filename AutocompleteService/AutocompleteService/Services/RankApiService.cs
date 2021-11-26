using System.Net.Http;
using System.Threading.Tasks;

namespace AutocompleteService.Services
{
    public class RankApiService : IRankApiService
    {
        private HttpClient _httpClient;

        public RankApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> GetRankSearchesAsync(string word)
        {
            string APIURL = $"http://localhost:5003/Rank/autocomplete/{word}";
            var response = await _httpClient.GetAsync(APIURL);
            return response;
        }
    }

    public interface IRankApiService
    {
        Task<HttpResponseMessage> GetRankSearchesAsync(string word);
    }
}
