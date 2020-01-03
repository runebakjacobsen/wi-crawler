using System;
using System.Linq;

namespace wi_crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var crawler = new Crawler("http://tv2.dk");

            //crawler.Crawl();

            //var test = new InvertedIndex();
            //test.BuildInvertedIndex();
            //var test = new BooleanQuery();
            // test.BooleanQueryProcessing("brand AND naturbrand");

            var test = new PageRanker();
            test.BuildMatrix();
            test.Transition();
        }
    }
}
