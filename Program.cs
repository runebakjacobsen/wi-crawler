using System.Threading.Tasks;
using System;

namespace wi_crawler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var crawler = new Crawler();
            await crawler.GetRobotsTxt("http://dr.dk");
        }
    }
}
