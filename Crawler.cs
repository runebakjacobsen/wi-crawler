using System.Threading;
using System.Linq;
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

        private void SetBaseUrl(string url)
        {
            Uri uri = new Uri(url);
            _baseUrl = uri.Host;
        }

        public string GetHTML(string url)
        {
            var httpClient = new HttpClient();
            var html = httpClient.GetStringAsync(url);

            return html.Result;
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

            return linkTos.Distinct().ToList();
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



        public void Crawl(string url)
        {


            // for each url in frontier -- stop when visitedUrls === MAX_URLS_VISIT
            //store html
            // add url to already visited 
            // wait 1 second 
            var html = GetHTML(url);
            // TODO - add entry to db
            visitedUrls.Add(url);
            Thread.Sleep(1000);





            //
        }

        private const int MAX_URLS_VISIT = 1000;
        private List<string> visitedUrls = new List<string>();
        public async Task Frontier()
        {


            // TODO Check if page passes near duplicate analysis, If it does not skip it, else add it 

            // TODO Extract “link-to” URLs 
            var linkTos = GetLinkTos(html);

            var notVisitedLinks = linkTos.Except(visitedUrls);

            var robotTxtHelper = new RobotTxtHelper(url);
            var disallowedUrls = robotTxtHelper.DisallowedUrls();

            var allowedLinks = notVisitedLinks.Except(disallowedUrls);

        }
    }
}