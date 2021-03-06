using System.Linq;
using System;
using System.Collections.Generic;
using HtmlAgilityPack;
namespace wi_crawler
{
    public class Frontier
    {
        private const int MAX_PAGES_SAME_DOMAIN = 10;

        public List<Uri> Seed { get; set; }
        public List<string> VisitedUrls { get; set; }

        public Frontier()
        {
            Seed = new List<Uri>();
            VisitedUrls = new List<string>();
        }

        public void AppendOutGoingLinks(Webpage webpage)
        {
            // TODO Check if page passes near duplicate analysis, If it does not skip it, else add it 

            var linkTos = GetLinkTos(webpage.HtmlContent);

            var notVisitedLinks = linkTos.Except(VisitedUrls);

            var robotTxtHelper = new RobotTxtHelper(webpage.Url);
            var disallowedUrls = robotTxtHelper.DisallowedUrls();

            // TODO Match robotstxt urls with regex, it is not absoulte paths -- or match on uri relative paths? 
            var allowedLinks = notVisitedLinks.Except(disallowedUrls).ToList();

            var allowedUris = TransformToUris(allowedLinks);

            UpdateSeed(allowedUris);
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
            if (url.StartsWith("mailto:") || url.StartsWith("tel:") || url.StartsWith("file:") || url.StartsWith("fb-messenger:") || url.StartsWith("javascript:"))
            {
                return false;
            }

            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                return false;
            }

            Uri uri = TryCreatingUri(url);

            if (uri == null)
            {
                return false;
            }

            if (url.EndsWith(".pdf"))
            {
                return false;
            }

            if (!uri.Host.EndsWith(".dk"))
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
                url = "http://" + GetBaseUrl(url) + url;
            }

            return url;
        }

        private string GetBaseUrl(string url)
        {
            Uri uri = new Uri(url);
            return uri.Host;
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

        private void UpdateSeed(List<Uri> allowedUris)
        {
            Dictionary<string, int> seedHostsCounts = SeedHostsAndCount();

            // TODO Refactor
            foreach (Uri uri in allowedUris)
            {
                string host = Common.NormalizeHost(uri.Host);
                bool isHostInSeed = seedHostsCounts.ContainsKey(host);

                if (!isHostInSeed)
                {
                    Seed.Add(uri);
                    seedHostsCounts = SeedHostsAndCount();
                }
                else if (seedHostsCounts[host] < MAX_PAGES_SAME_DOMAIN && !IsUriInSeed(uri))
                {
                    Seed.Add(uri);
                    seedHostsCounts = SeedHostsAndCount();
                }
            }
        }

        private Dictionary<string, int> SeedHostsAndCount()
        {
            return Seed.GroupBy(x => Common.NormalizeHost(x.Host)).ToDictionary(x => x.Key, x => x.Count());
        }

        private bool IsUriInSeed(Uri uri)
        {
            foreach (Uri SeedUri in Seed)
            {
                var result = CompareUris(uri, SeedUri);
                var isUrisEqual = result == 0;

                if (isUrisEqual)
                {
                    return true;
                }

            }
            return false;
        }

        public int CompareUris(Uri uri1, Uri uri2)
        {
            return Uri.Compare(uri1, uri2, UriComponents.Host | UriComponents.PathAndQuery, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        }
    }
}