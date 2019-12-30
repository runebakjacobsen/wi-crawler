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

        public List<string> Stemmer(string content)
        {
            string[] words = content.Split(' ');

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
    }
}