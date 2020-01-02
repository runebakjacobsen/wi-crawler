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
                var query1 = query.Substring(0, query.IndexOf("AND")).Trim();
                var query2 = query.Substring(query.IndexOf("AND") + 3).Trim();

                using var db = new CrawlingContext();

                if (db.TermIndexes.Any(x => x.Term.Equals(query1)) && db.TermIndexes.Any(x => x.Term.Equals(query2)))
                {

                    var termindex1 = db.TermIndexes.First(x => x.Term.Equals(query1));
                    var termIndex2 = db.TermIndexes.First(x => x.Term.Equals(query2));

                    var res = BooleanAndQuery(termindex1, termIndex2);
                }
                else
                {
                    System.Console.WriteLine("No results");
                    return;
                }

            }
            else if (query.Contains("OR"))
            {
                var query1 = query.Substring(0, query.IndexOf("OR")).Trim();
                var query2 = query.Substring(query.IndexOf("OR") + 3).Trim();
                using var db = new CrawlingContext();

                if (db.TermIndexes.Any(x => x.Term.Equals(query1)) && db.TermIndexes.Any(x => x.Term.Equals(query2)))
                {

                    var termindex1 = db.TermIndexes.First(x => x.Term.Equals(query1));
                    var termIndex2 = db.TermIndexes.First(x => x.Term.Equals(query2));

                    var res = BooleanOrQuery(termindex1, termIndex2);
                }
                else
                {
                    System.Console.WriteLine("No results");
                    return;
                }
            }
            else if (query.StartsWith("NOT"))
            {
                var query1 = query.Substring(3).Trim();
                using var db = new CrawlingContext();
                if (db.TermIndexes.Any(x => x.Term.Equals(query1)))
                {
                    var termindex = db.TermIndexes.First(x => x.Term.Equals(query1));

                    var res = BooleanNotQuery(termindex);
                }
                else
                {
                    System.Console.WriteLine("No results");
                    return;
                }
            }
        }

        public List<int> BooleanAndQuery(TermIndex termIndex1, TermIndex termIndex2)
        {
            var index1 = termIndex1.WebpageIds;
            var index2 = termIndex2.WebpageIds;

            return index1.Intersect(index2).ToList();
        }

        public List<int> BooleanOrQuery(TermIndex termIndex1, TermIndex termIndex2)
        {
            var list1 = termIndex1.WebpageIds.ToList();
            var list2 = termIndex2.WebpageIds.ToList();

            return list1.Union(list2).OrderBy(x => x).ToList();
        }

        public List<int> BooleanNotQuery(TermIndex termIndex)
        {
            using var db = new CrawlingContext();

            List<int> allWebpageIds = db.Webpages.Select(x => x.WebpageId).ToList();

            return allWebpageIds.Except(termIndex.WebpageIds).ToList();
        }
    }
}
