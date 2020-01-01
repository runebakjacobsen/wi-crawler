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
        private readonly IStemmer _stemmer = new DanishStemmer();
        private readonly Dictionary<string, LinkedList<int>> _invertedIndex = new Dictionary<string, LinkedList<int>>();
        private readonly List<TermFrequencyVector> _termFrequencyVectors = new List<TermFrequencyVector>();

        private List<string> Stemmer(string content)
        {
            string[] words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            List<string> stemmedWords = _stemmer.GetSteamWords(words).ToList();

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
                var content = RemoveStopWords(webpage.Content);
                var stemmedContent = Stemmer(content);
                var termsForWebpage = GetTermsSequencesForWebpage(stemmedContent, webpage.WebpageId);

                termSequences.AddRange(termsForWebpage);
            }
            return termSequences;
        }

        public void TfIdfIndex()
        {
            using var db = new CrawlingContext();
            List<Webpage> webpages = db.Webpages.OrderBy(x => x.WebpageId).ToList();

            foreach (Webpage webpage in webpages)
            {
                BuildTfIdfIndex(webpage);
            }
        }

        private void BuildTfIdfIndex(Webpage webpage)
        {
            var content = RemoveStopWords(webpage.Content);
            var stemmedContent = Stemmer(content);

            Dictionary<string, int> termFrequencyLookup = CountTermFrequencies(stemmedContent);

            foreach (var term in termFrequencyLookup)
            {
                var weighting = TfIdfWeighting(term.Value, stemmedContent.Count());
                var documentFrequency = new DocumentFrequency(webpage.WebpageId, weighting);
                AddOrUpdateTermFrequencyVectors(term, documentFrequency);
            }
        }

        private Dictionary<string, int> CountTermFrequencies(List<string> content)
        {
            var termFrequencies = new Dictionary<string, int>();

            foreach (string word in content)
            {
                if (termFrequencies.ContainsKey(word))
                {
                    termFrequencies[word]++;
                }
                else
                {
                    termFrequencies.Add(word, 1);
                }
            }

            return termFrequencies;
        }

        private void AddOrUpdateTermFrequencyVectors(KeyValuePair<string, int> term, DocumentFrequency documentFrequency)
        {
            bool isTermsEqual(TermFrequencyVector x) => x.Term.Equals(term.Key);

            if (_termFrequencyVectors.Exists(isTermsEqual))
            {
                var matchingTermFrequencyVector = _termFrequencyVectors.Find(isTermsEqual);
                matchingTermFrequencyVector.DocumentFrequencies.Add(documentFrequency);
                return;
            }

            var documentFrequencies = new List<DocumentFrequency>() { documentFrequency };
            var termFrequencyVector = new TermFrequencyVector() { Term = term.Key, DocumentFrequencies = documentFrequencies };
            _termFrequencyVectors.Add(termFrequencyVector);
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