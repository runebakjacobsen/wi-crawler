using System;
using System.Collections.Generic;
using System.Linq;

namespace wi_crawler
{
    public class BooleanQuery
    {
        public void BooleanQueryProcessing(string query)
        {
            if (query.Contains("AND"))
            {
                ProcessQuery(query, "AND", BooleanAndQuery);
            }
            else if (query.Contains("OR"))
            {
                ProcessQuery(query, "OR", BooleanOrQuery);
            }
            else if (query.StartsWith("NOT"))
            {
                ProcessNotQuery(query);
            }
        }

        private void ProcessQuery(string query, string queryType, Func<TermIndex, TermIndex, List<int>> queryTypeFunc)
        {
            var queries = SplitQuery(query, queryType);
            var query1 = queries.ElementAt(0);
            var query2 = queries.ElementAt(1);

            using var db = new CrawlingContext();

            bool isInDb(string query) => db.TermIndexes.Any(x => x.Term.Equals(query));

            if (isInDb(query1) && isInDb(query2))
            {
                var termindex1 = db.TermIndexes.First(x => x.Term.Equals(query1));
                var termIndex2 = db.TermIndexes.First(x => x.Term.Equals(query2));

                var res = queryTypeFunc(termindex1, termIndex2);
            }
            else
            {
                Console.WriteLine("No results");
                return;
            }
        }

        private List<string> SplitQuery(string query, string splitWord)
        {
            var query1 = query.Substring(0, query.IndexOf(splitWord)).Trim();
            var query2 = query.Substring(query.IndexOf(splitWord) + splitWord.Count()).Trim();

            return new List<string>() { query1, query2 };
        }

        private List<int> BooleanAndQuery(TermIndex termIndex1, TermIndex termIndex2)
        {
            var index1 = termIndex1.WebpageIds;
            var index2 = termIndex2.WebpageIds;

            return index1.Intersect(index2).ToList();
        }

        private List<int> BooleanOrQuery(TermIndex termIndex1, TermIndex termIndex2)
        {
            var index1 = termIndex1.WebpageIds;
            var index2 = termIndex2.WebpageIds;

            return index1.Union(index2).OrderBy(x => x).ToList();
        }

        private void ProcessNotQuery(string query)
        {
            var query1 = query.Substring(3).Trim();

            using var db = new CrawlingContext();

            bool isInDb(string query) => db.TermIndexes.Any(x => x.Term.Equals(query));

            if (isInDb(query1))
            {
                var termindex = db.TermIndexes.First(x => x.Term.Equals(query1));

                var res = BooleanNotQuery(termindex);
            }
            else
            {
                Console.WriteLine("No results");
                return;
            }
        }

        private List<int> BooleanNotQuery(TermIndex termIndex)
        {
            using var db = new CrawlingContext();

            List<int> allWebpageIds = db.Webpages.Select(x => x.WebpageId).ToList();

            return allWebpageIds.Except(termIndex.WebpageIds).ToList();
        }
    }
}
