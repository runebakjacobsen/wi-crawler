using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
namespace wi_crawler
{
    public class Crawler
    {
        public Crawler(string seedUrl)
        {
            SetBaseUrl(seedUrl);
        }


        private string _baseUrl;
        public string GetHTML(string url)
        {
            var httpClient = new HttpClient();
            var html = httpClient.GetStringAsync(url);

            return html.Result;
        }

        private void SetBaseUrl(string url)
        {
            Uri uri = new Uri(url);
            _baseUrl = uri.Host;
        }

        public string GetContent(string html)
        {
            var htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(html);

            return htmlDocument.ParsedText;
            // TODO - escape html

        }

        public List<string> GetLinkTos(string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var linkTos = new List<string>();

            foreach (HtmlNode link in htmlDocument.DocumentNode.SelectNodes("//a[@href]"))
            {
                string hrefValue = link.GetAttributeValue("href", string.Empty);
                // TODO Refactor this
                if (!IsWebUrl(hrefValue))
                {
                    continue;
                }
                var normalizedUrl = NormalizeUrl(hrefValue);
                linkTos.Add(normalizedUrl);
            }

            return linkTos;
        }

        private bool IsWebUrl(string url)
        {
            if (url.StartsWith("mailto:") || url.StartsWith("tel:"))
            {
                return false;
            }

            return true;
        }

        private string NormalizeUrl(string url)
        {
            if (url.StartsWith("//"))
            {
                url = "http:" + url;
            }

            if (!url.Contains("://"))
            {
                url = "http://" + _baseUrl + url;
            }

            return url;
        }

        public string GetRobotsTxt(string baseUrl)
        {
            var httpClient = new HttpClient();
            var robots = httpClient.GetStringAsync($"{baseUrl}/robots.txt");

            return robots.Result;
        }

        public async Task Frontier()
        {
            // TODO store list with already visited domains

            // TODO Check if page passes near duplicate analysis, If it does not skip it, else add it 

            // TODO Extract “link-to” URLs 
            // Normalize URL
            // b. Check that it passes certain URL filter tests. E.g.:
            // • Focused crawl: only crawl .dk
            // • Obey robots.txt (freshness caveat)
            // c. Check that not already in frontier
        }
    }
}