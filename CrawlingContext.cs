using Microsoft.EntityFrameworkCore;

namespace wi_crawler
{
    public class CrawlingContext : DbContext
    {
        public DbSet<Webpage> Webpages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
           => options.UseSqlite("Data Source=crawler.db");
    }

    public class Webpage
    {
        public int WebpageId { get; set; }
        public string Html { get; set; }
        public string Url { get; set; }
        public string BaseDomain { get; set; }

    }
}