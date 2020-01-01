using Microsoft.EntityFrameworkCore;

namespace wi_crawler
{
    public class CrawlingContext : DbContext
    {
        public DbSet<Webpage> Webpages { get; set; }
        public DbSet<TermIndex> TermIndexes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
           => options.UseSqlite("Data Source=crawler.db");
    }
}