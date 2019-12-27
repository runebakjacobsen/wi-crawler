using System.Linq;
using System.Net.Http;
using System;
using System.Collections.Generic;
namespace wi_crawler
{
    public class RobotTxtHelper
    {
        private readonly string _baseUrl;
        private readonly string _requestUrl;

        public RobotTxtHelper(string requestUrl)
        {
            _requestUrl = requestUrl;

            var uri = new Uri(_requestUrl);
            _baseUrl = uri.Scheme + "://" + uri.Host;
        }

        private string GetRobotsTxt()
        {
            var httpClient = new HttpClient();
            var robots = httpClient.GetStringAsync($"{_baseUrl}/robots.txt");
            return robots.Result;
        }

        public List<string> DisallowedUrls()
        {
            var robotsTxt = GetRobotsTxt();

            var disallowedPaths = FindDisallowedPaths(robotsTxt);

            // Robots.txt only contains relative paths, so we preprend the _baseUrl to all paths
            var disallowedUrls = disallowedPaths.Select(val => _baseUrl + val).ToList();

            return disallowedUrls;
        }

        private List<string> GetDisallowedPaths(string robots)
        {
            robots = robots.ToLower();

            if (!robots.Contains("user-agent: *"))
            {
                return new List<string>();
            }

            string trimmedRobotsTxt = RemoveOtherUserAgents(robots);

            return FindDisallowedPaths(trimmedRobotsTxt);
        }

        private string RemoveOtherUserAgents(string robotsTxt)
        {
            string targetUserAgent = "user-agent: *";
            int indexUserAgentAll = robotsTxt.IndexOf(targetUserAgent, StringComparison.Ordinal);
            int offsetCurrentAgent = indexUserAgentAll + targetUserAgent.Length;

            int indexNextAgent = robotsTxt.IndexOf("user-agent:", offsetCurrentAgent, StringComparison.Ordinal);

            if (indexNextAgent == -1)
            {
                indexNextAgent = robotsTxt.Length;
            }

            return robotsTxt[indexUserAgentAll..indexNextAgent];
        }

        private List<string> FindDisallowedPaths(string trimmedRobotsTxt)
        {
            List<string> robotsSplitted = trimmedRobotsTxt.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            List<string> disallowedPaths = robotsSplitted
                .FindAll(x => x.StartsWith("disallow:"))
                .Select(x => x.Replace("disallow: ", string.Empty).Trim())
                .ToList();

            return disallowedPaths;
        }
    }
}