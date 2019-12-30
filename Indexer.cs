using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Annytab.Stemmer;

namespace wi_crawler
{
    public class Indexer
    {
        private readonly IStemmer stemmer = new DanishStemmer();

        private List<string> Stemmer(string content)
        {
            string[] words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            List<string> stemmedWords = stemmer.GetSteamWords(words).ToList();

            return stemmedWords;
        }

        private string RemoveStopWords(string content)
        {
            string stopwordsTxt = File.ReadAllText("./danish-stop-words.txt");

            var stopwords = stopwordsTxt.Split(Environment.NewLine, StringSplitOptions.None);
            stopwords = stopwords.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            var contentWithoutStopwords = content.Split(" ").Except(stopwords);

            return string.Join(" ", contentWithoutStopwords);
        }

        public void InvertedIndex()
        {
            List<KeyValuePair<string, int>> termSequence = new List<KeyValuePair<string, int>>();

            using var db = new CrawlingContext();
            var webpages = db.Webpages.OrderBy(x => x.WebpageId);

            foreach (Webpage webpage in webpages)
            {
                var content = RemoveStopWords(webpage.Content);
                var stemmedContent = Stemmer(content);

                foreach (string word in stemmedContent)
                {
                    termSequence.Add(new KeyValuePair<string, int>(word, webpage.WebpageId));
                }
            }

            var sorted = termSequence.OrderBy(x => x.Key).ThenBy(x => x.Value).ToList();

            Dictionary<string, LinkedList<int>> invertedIndex = new Dictionary<string, LinkedList<int>>();

            foreach (var pair in sorted)
            {
                if (invertedIndex.ContainsKey(pair.Key))
                {
                    invertedIndex[pair.Key].AddLast(pair.Value);
                }
                else
                {
                    var list = new LinkedList<int>();
                    list.AddLast(pair.Value);
                    invertedIndex.Add(pair.Key, list);
                }
            }
        }
    }
}