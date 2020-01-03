using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace wi_crawler
{
    public class PageRanker
    {
        private double[,] transitionProbabilityMatrix;
        private double[] probabilityDistribution;

        public void BuildMatrix()
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

            //normalize it, we have more links in html thhan in db, we need to make the matrix rows sum to 1
            for (int i = 0; i < rowLength; i++)
            {
                int count = 0;
                for (int j = 0; j < colLength; j++)
                {
                    if (matrix[i, j] != 0)
                    {
                        count++;
                    }
                }

                for (int j = 0; j < colLength; j++)
                {
                    if (matrix[i, j] != 0)
                    {
                        matrix[i, j] = (double)1 / (double)count;
                    }
                }
            }

            transitionProbabilityMatrix = matrix;
            probabilityDistribution = new double[rowLength];
            probabilityDistribution[0] = 1;

        }


        public void Transition()
        {

            var matrixBuilder = Matrix<double>.Build;
            var matrix = matrixBuilder.DenseOfArray(transitionProbabilityMatrix);
            var resarray = new double[probabilityDistribution.Count()];
            while (!Enumerable.SequenceEqual(probabilityDistribution, resarray))
            {
                probabilityDistribution = resarray;
                var vectorBuilder = Vector<double>.Build;
                var vector = vectorBuilder.DenseOfArray(probabilityDistribution);
                var res = vector * matrix;
                resarray = res.ToArray();
            }
        }
    }
}