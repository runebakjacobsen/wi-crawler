using System;
using System.Linq;

namespace wi_crawler
{
    public class PageRanker
    {
        //* Just make it possible to get the pagerank for domain
        //* Lets save it in the db or something? Then we can always implement it later.. 

        double[,] transitionProbabilityMatrix;

        public void buildMatrix()
        {
            using var db = new CrawlingContext();

            var webpages = db.Webpages.ToList();

            var matrix = new double[webpages.Count(), webpages.Count()];
            int rowLength = matrix.GetLength(0);
            int colLength = matrix.GetLength(1);
            for (int i = 0; i < rowLength; i++)
            {
                var rowWebpage = webpages.Find(x => x.WebpageId == i + 1);
                var rowUrl = new Uri(rowWebpage.Url);
                var frontier = new Frontier();
                var outgoingLinks = frontier.GetLinkTos(rowWebpage.HtmlContent).Select(x => new Uri(x)).ToList();
                for (int j = 0; j < colLength; j++)
                {
                    var colWebpage = webpages.Find(x => x.WebpageId == j + 1);
                    var colUrl = new Uri(colWebpage.Url);
                    double probability = 0;

                    foreach (var link in outgoingLinks)
                    {
                        if (frontier.CompareUris(link, colUrl) == 0)
                        {
                            probability = (double)1 / (double)outgoingLinks.Count();
                        }
                    }


                    matrix[i, j] = probability;
                }
            }

            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    Console.Write(string.Format("{0} ", matrix[i, j]));
                }
                Console.Write(Environment.NewLine + Environment.NewLine);
            }
            Console.ReadLine();
        }
    }
}