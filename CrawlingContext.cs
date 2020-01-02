using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace wi_crawler
{
    public class CrawlingContext : DbContext
    {
        public DbSet<Webpage> Webpages { get; set; }
        public DbSet<TermIndex> TermIndexes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
           => options.UseSqlite("Data Source=crawler.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Func<string, LinkedList<int>> splitStringToLinkedList = (string s) => new LinkedList<int>(s.Split(",").Select(x => int.Parse(x)));

            var converter = new ValueConverter<LinkedList<int>, string>(
                v => string.Join(",", v),
                v => splitStringToLinkedList(v));

            modelBuilder.Entity<TermIndex>().Property(x => x.WebpageIds).HasConversion(converter);
        }

    }
}