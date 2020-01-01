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
        // TODO - make keypairs/dics to classes
        private readonly IStemmer stemmer = new DanishStemmer();
        private readonly Dictionary<string, LinkedList<int>> _invertedIndex = new Dictionary<string, LinkedList<int>>();

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

        private double TfIdfWeighting(double termFrequency, int totalWordsInDoc)
        {
            return LogTermFrequency(termFrequency) * InverseDocumentFrequency(termFrequency, totalWordsInDoc);
        }
        private double LogTermFrequency(double termFrequency)
        {
            if (termFrequency > 0)
            {
                return 1 + Math.Log10(termFrequency);
            }

            return 0;
        }
        private double InverseDocumentFrequency(double termFrequency, int totalWordsInDoc)
        {
            return Math.Log10(totalWordsInDoc / termFrequency);
        }
        public void InvertedIndex()
        {
            // TODO Save to db? 
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
                TermFrequencyVectors(webpage);


                var content = RemoveStopWords(webpage.Content);
                var stemmedContent = Stemmer(content);
                var termsForWebpage = GetTermsSequencesForWebpage(stemmedContent, webpage.WebpageId);

                termSequences.AddRange(termsForWebpage);
            }
            var qq = termFrequencyVectors.OrderByDescending(x => x.DocumentFrequencies.Count());
            return termSequences;
        }

        readonly List<TermFrequencyVector> termFrequencyVectors = new List<TermFrequencyVector>();
        public void TermFrequencyVectors(Webpage webpage)
        {

            var content = RemoveStopWords(webpage.Content);
            var stemmedContent = Stemmer(content);

            var terms = new Dictionary<string, int>();
            foreach (string word in stemmedContent)
            {
                if (terms.ContainsKey(word))
                {
                    terms[word]++;
                }
                else
                {
                    terms.Add(word, 1);
                }
            }

            foreach (var term in terms)
            {

                if (termFrequencyVectors.Exists(x => x.Term.Equals(term.Key)))
                {
                    var test = termFrequencyVectors.Find(x => x.Term.Equals(term.Key));
                    termFrequencyVectors.Remove(test);
                    var doc = new DocumentFrequency() { WebpageId = webpage.WebpageId, TermFrequency = TfIdfWeighting(term.Value, stemmedContent.Count()) };
                    test.DocumentFrequencies.Add(doc);
                    termFrequencyVectors.Add(test);
                }
                else
                {
                    var doc = new DocumentFrequency() { WebpageId = webpage.WebpageId, TermFrequency = TfIdfWeighting(term.Value, stemmedContent.Count()) };
                    var docList = new List<DocumentFrequency>() { doc };
                    var test1 = new TermFrequencyVector() { Term = term.Key, DocumentFrequencies = docList };
                    termFrequencyVectors.Add(test1);
                }
            }
        }

        private List<KeyValuePair<string, int>> GetTermsSequencesForWebpage(List<string> stemmedContent, int id)
        {
            List<KeyValuePair<string, int>> termSequences = new List<KeyValuePair<string, int>>();

            foreach (string word in stemmedContent)
            {
                termSequences.Add(new KeyValuePair<string, int>(word, id));
            }
            var test = termSequences.OrderBy(x => x.Key).ToList();
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