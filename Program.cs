using System.Threading.Tasks;
using System;

namespace wi_crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var crawler = new Crawler("http://dr.dk");

            crawler.Crawl();

        }
    }
}
