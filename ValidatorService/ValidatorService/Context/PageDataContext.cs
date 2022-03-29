using ValidatorService.Models;
using Microsoft.EntityFrameworkCore;

namespace ValidatorService.Context
{
    public class PageDataContext : DbContext
    {
        public DbSet<PageData> PageData { get; set; }
        public PageDataContext(DbContextOptions<PageDataContext> options)
           : base(options)
        { }
    }
}
