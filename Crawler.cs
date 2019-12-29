using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Net.Http;
using HtmlAgilityPack;

namespace wi_crawler
{
    public class Crawler
    {
        public Crawler(string seedUrl)
        {
            SetBaseUrl(seedUrl);
            frontier.Add(new Uri(seedUrl));
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
            Task<string> html = httpClient.GetStringAsync(url);

            return TryGettingHtmlResult(html);
        }

        public string TryGettingHtmlResult(Task<string> html)
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

            var hrefs = htmlDocument.DocumentNode.SelectNodes("//a[@href]");

            if (hrefs == null)
            {
                return linkTos;
            }

            foreach (HtmlNode link in htmlDocument.DocumentNode.SelectNodes("//a[@href]"))
            {
                string hrefValue = link.GetAttributeValue("href", string.Empty);
                // TODO Refactor this
                if (IsWebUrl(hrefValue))
                {
                    var normalizedUrl = NormalizeUrl(hrefValue);
                    linkTos.Add(normalizedUrl);
                }
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

        public void Crawl()
        {
            // for each url in frontier -- stop when visitedUrls === MAX_URLS_VISIT
            //store html
            // add url to already visited 
            // wait 1 second 
            int incrementor = 0;
            while (visitedUrls.Count() < MAX_URLS_VISIT && frontier.Count() > incrementor)
            {
                var url = frontier[incrementor].AbsoluteUri;
                System.Console.WriteLine(url);
                var html = GetHTML(url);
                visitedUrls.Add(url);
                Frontier(url, html);
                Thread.Sleep(1000);
                incrementor++;
            }

            // TODO - add entry to db
        }

        private const int MAX_URLS_VISIT = 1000;
        private const int MAX_PAGES_SAME_DOMAIN = 5;
        private readonly List<string> visitedUrls = new List<string>();

        private readonly List<Uri> frontier = new List<Uri>();
        public void Frontier(string url, string html)
        {
            // TODO Check if page passes near duplicate analysis, If it does not skip it, else add it 

            var linkTos = GetLinkTos(html);

            var notVisitedLinks = linkTos.Except(visitedUrls);

            var robotTxtHelper = new RobotTxtHelper(url);
            var disallowedUrls = robotTxtHelper.DisallowedUrls();

            // TODO Match robotstxt urls with regex, it is not absoulte paths 
            var allowedLinks = notVisitedLinks.Except(disallowedUrls).ToList();

            var allowedUris = TransformToUris(allowedLinks);

            UpdateFrontier(allowedUris);
        }

        private bool HostInFrontier(Uri x)
        {
            return frontier.Any(y => NormalizeHost(y.Host) == NormalizeHost(x.Host));
        }
        private void UpdateFrontier(List<Uri> allowedUris)
        {
            var frontierHostCounts = FrontierDicWithHostAndCount();

            // TODO Refactor
            foreach (Uri uri in allowedUris)
            {
                string host = NormalizeHost(uri.Host);
                bool isHostInFrontier = frontierHostCounts.ContainsKey(host);

                if (!isHostInFrontier)
                {
                    frontier.Add(uri);
                    frontierHostCounts = FrontierDicWithHostAndCount();
                }
                else if (frontierHostCounts[host] < MAX_PAGES_SAME_DOMAIN && !frontier.Contains(uri))
                {
                    frontier.Add(uri);
                    // check that link is not the same......
                    frontierHostCounts = FrontierDicWithHostAndCount();
                }
            }
        }

        private Dictionary<string, int> FrontierDicWithHostAndCount()
        {
            return frontier.GroupBy(x => NormalizeHost(x.Host)).ToDictionary(x => x.Key, x => x.Count());
        }

        private List<Uri> TransformToUris(List<string> links)
        {
            List<Uri> uris = new List<Uri>();

            foreach (string link in links)
            {
                var uri = TryCreatingUri(link);

                if (uri != null)
                {
                    uris.Add(uri);
                }
            }
            return uris;
        }

        private Uri TryCreatingUri(string link)
        {
            try
            {
                return new Uri(link);
            }
            catch (UriFormatException)
            {
                return null;
            }
        }

        private string NormalizeHost(string host)
        {
            if (host.StartsWith("www."))
            {
                return host.Remove(0, 4);
            }
            return host;
        }
    }
}