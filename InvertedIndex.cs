using System.Collections.Generic;
using System.Linq;
namespace wi_crawler
{
    public class InvertedIndex : Indexer
    {
        private readonly List<TermIndex> _invertedIndex = new List<TermIndex>();

        public void BuildInvertedIndex()
        {
            using var db = new CrawlingContext();
            List<Webpage> webpages = db.Webpages.OrderBy(x => x.WebpageId).ToList();

            List<TermSequence> termSequences = GenerateTermSequences(webpages);

            var sortedTermSequences = termSequences.OrderBy(x => x.Term).ThenBy(x => x.Webpage.WebpageId).ToList();

            sortedTermSequences.ForEach(x => AddToInvertedIndex(x));
        }

        private List<TermSequence> GenerateTermSequences(List<Webpage> webpages)
        {
            List<TermSequence> termSequences = new List<TermSequence>();

            foreach (Webpage webpage in webpages)
            {
                var content = RemoveStopWords(webpage.Content);
                var stemmedContent = Stemmer(content);
                var termsForWebpage = GetTermsSequencesForWebpage(stemmedContent, webpage);

                termSequences.AddRange(termsForWebpage);
            }
            return termSequences;
        }

        private List<TermSequence> GetTermsSequencesForWebpage(List<string> stemmedContent, Webpage webpage)
        {
            List<TermSequence> termSequences = new List<TermSequence>();

            foreach (string word in stemmedContent)
            {
                termSequences.Add(new TermSequence(word, webpage));
            }

            return termSequences;
        }

        private void AddToInvertedIndex(TermSequence termSequence)
        {
            using var db = new CrawlingContext();

            if (_invertedIndex.Exists(x => x.Term.Equals(termSequence.Term)))
            {
                var termIndex = _invertedIndex.Find(x => x.Term.Equals(termSequence.Term));
                if (!termIndex.WebpageIds.Contains(termSequence.Webpage.WebpageId))
                {
                    termIndex.WebpageIds.AddLast(termSequence.Webpage.WebpageId);
                    db.Update(termIndex);
                    db.SaveChanges();

                }
            }
            else
            {
                var termIndex = new TermIndex
                {
                    Term = termSequence.Term,

                };
                termIndex.WebpageIds.AddLast(termSequence.Webpage.WebpageId);

                _invertedIndex.Add(termIndex);
                db.Add(termIndex);
                db.SaveChanges();

            }
        }

        public void BooleanQueryProcessing(string query)
        {
            if (query.Contains("AND"))
            {
                var query1 = query.Substring(0, query.IndexOf("AND")).Trim();
                var query2 = query.Substring(query.IndexOf("AND") + 3).Trim();

                using var db = new CrawlingContext();
                var termindex1 = db.TermIndexes.First(x => x.Term.Equals(query1));
                var termIndex2 = db.TermIndexes.First(x => x.Term.Equals(query2));

                var res = BooleanAndQuery(termindex1, termIndex2);

            }
        }

        public List<int> BooleanAndQuery(TermIndex termIndex1, TermIndex termIndex2)
        {
            var answer = new List<int>();

            int incrementor1 = 0;
            int incrementor2 = 0;
            var index1 = termIndex1.WebpageIds.ToArray();
            var index2 = termIndex2.WebpageIds.ToArray();
            while (index1.Count() > incrementor1 && index2.Count() > incrementor2)
            {
                if (index1[incrementor1] == index2[incrementor2])
                {
                    answer.Add(index1[incrementor1]);
                    incrementor1++;
                    incrementor2++;
                }
                else if (index1[incrementor1] < index2[incrementor2])
                {
                    incrementor1++;
                }
                else
                {
                    incrementor2++;
                }
            }

            return answer;
        }
    }
}