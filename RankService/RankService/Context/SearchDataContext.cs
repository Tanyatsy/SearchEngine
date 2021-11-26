using Microsoft.EntityFrameworkCore;
using RankService.Models;

namespace RankService.Context
{
    public class SearchDataContext : DbContext
    {
        public DbSet<SearchData> SearchData { get; set; }
        public SearchDataContext(DbContextOptions<SearchDataContext> options)
           : base(options)
        { }
    }
}
