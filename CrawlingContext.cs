using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace wi_crawler
{
    public class CrawlingContext : DbContext
    {
        public DbSet<Webpage> Webpages { get; set; }
        public DbSet<TermIndex> TermIndexes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
           => options.UseSqlite("Data Source=crawler.db");
    }

    public class Webpage
    {
        public int WebpageId { get; set; }
        public string HtmlContent { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
        public string BaseDomain { get; set; }

    }

    public class TermIndex
    {
        [Key]
        public string Term { get; set; }
        public LinkedList<Webpage> Webpages { get; } = new LinkedList<Webpage>();
    }
}