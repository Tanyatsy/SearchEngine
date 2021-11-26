using RankService.Context;
using RankService.Models;
using System.Collections.Generic;
using System.Linq;

namespace RankService.Repositories
{
    public class SearchDataRepository : ISearchDataRepository
    {
        private readonly SearchDataContext _context;

        public SearchDataRepository(SearchDataContext dbContext)
        {
            _context = dbContext;
        }

        public void Create(SearchData data)
        {
            _context.SearchData.Add(data);
            _context.SaveChanges();
        }

        public List<string> FindByWord(string word)
        {
            return _context.SearchData
                .Where(data => data.Text == word)
                .Select(x => x.Text)
                .ToList();
        }
    }

    public interface ISearchDataRepository
    {
        void Create(SearchData data);
        List<string> FindByWord(string word);
    }
}
