using RankService.Context;
using RankService.Models;
using System.Collections.Generic;

namespace RankService.Repositories
{
    public class SearchDataService : ISearchDataService
    {
        private readonly ISearchDataRepository searchDataRepository;

        public SearchDataService(ISearchDataRepository searchDataRepository)
        {
            this.searchDataRepository = searchDataRepository;
        }

        public void Create(SearchData data)
        {
            searchDataRepository.Create(data);
        }

        public List<string> FindByWord(string word)
        {
            return searchDataRepository.FindByWord(word);
        }

    }

    public interface ISearchDataService
    {
        void Create(SearchData data);
        List<string> FindByWord(string word);
    }
}
