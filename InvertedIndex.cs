using System.Collections.Generic;
using System.Linq;
namespace wi_crawler
{
    public class InvertedIndex : Indexer
    {
        private readonly Dictionary<string, LinkedList<int>> _invertedIndex = new Dictionary<string, LinkedList<int>>();

        public void BuildInvertedIndex()
        {
            using var db = new CrawlingContext();
            List<Webpage> webpages = db.Webpages.OrderBy(x => x.WebpageId).ToList();

            List<KeyValuePair<string, int>> termSequences = GenerateTermSequences(webpages);

            var sortedTermSequences = termSequences.OrderBy(x => x.Key).ThenBy(x => x.Value).ToList();

            sortedTermSequences.ForEach(x => AddToInvertedIndex(x));
        }

        private List<KeyValuePair<string, int>> GenerateTermSequences(List<Webpage> webpages)
        {
            List<KeyValuePair<string, int>> termSequences = new List<KeyValuePair<string, int>>();

            foreach (Webpage webpage in webpages)
            {
                var content = RemoveStopWords(webpage.Content);
                var stemmedContent = Stemmer(content);
                var termsForWebpage = GetTermsSequencesForWebpage(stemmedContent, webpage.WebpageId);

                termSequences.AddRange(termsForWebpage);
            }
            return termSequences;
        }

        private List<KeyValuePair<string, int>> GetTermsSequencesForWebpage(List<string> stemmedContent, int id)
        {
            List<KeyValuePair<string, int>> termSequences = new List<KeyValuePair<string, int>>();

            foreach (string word in stemmedContent)
            {
                termSequences.Add(new KeyValuePair<string, int>(word, id));
            }

            return termSequences;
        }

        private void AddToInvertedIndex(KeyValuePair<string, int> termSequence)
        {
            if (_invertedIndex.ContainsKey(termSequence.Key))
            {
                _invertedIndex[termSequence.Key].AddLast(termSequence.Value);
            }
            else
            {
                var list = new LinkedList<int>();
                list.AddLast(termSequence.Value);
                _invertedIndex.Add(termSequence.Key, list);
            }
        }
    }
}