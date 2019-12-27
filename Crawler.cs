using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
namespace wi_crawler
{
    public class Crawler
    {
        public async Task<string> GetHTML(string url)
        {
            var httpClient = new HttpClient();
            var html = httpClient.GetStringAsync(url);

            return html.Result;
        }

        public async Task<string> GetRobotsTxt(string url)
        {
            var httpClient = new HttpClient();
            var robots = httpClient.GetStringAsync($"{url}/robots.txt");

            return robots.Result;
        }
    }
}