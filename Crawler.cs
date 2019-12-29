using System.Web;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System;
using System.Net.Http;
using HtmlAgilityPack;

namespace wi_crawler
{
    public class Crawler
    {
        private const int MAX_URLS_VISIT = 1000;
        private readonly Frontier frontier;

        public Crawler(string seedUrl)
        {
            frontier = new Frontier();
            frontier.Seed.Add(new Uri(seedUrl));

            using var db = new CrawlingContext();
            db.Database.EnsureCreated();
        }

        public void Crawl()
        {
            using var db = new CrawlingContext();

            int incrementor = 0;
            while (frontier.VisitedUrls.Count() < MAX_URLS_VISIT)
            {
                var webpage = MakeWebpageObject(frontier.Seed[incrementor]);

                Console.WriteLine($"{incrementor}  {webpage.Url}");

                db.Add(webpage);
                db.SaveChanges();

                frontier.VisitedUrls.Add(webpage.Url);
                frontier.AppendOutGoingLinks(webpage);
                Thread.Sleep(1000);
                incrementor++;
            }
        }

        private Webpage MakeWebpageObject(Uri uri)
        {
            string url = uri.AbsoluteUri;
            string host = Common.NormalizeHost(uri.Host);
            string html = GetHTML(url);

            return new Webpage()
            {
                HtmlContent = html,
                Content = StripHtmlTags(html),
                Url = url,
                BaseDomain = host
            };
        }

        private string GetHTML(string url)
        {
            var httpClient = new HttpClient();
            Task<string> html = httpClient.GetStringAsync(url);

            return TryGettingHtmlResult(html);
        }

        private string TryGettingHtmlResult(Task<string> html)
        {
            try
            {
                return html.Result;
            }
            catch (AggregateException)
            {
                return "";
            }
        }

        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return HttpUtility.HtmlDecode(doc.DocumentNode.InnerText);
        }
    }
}