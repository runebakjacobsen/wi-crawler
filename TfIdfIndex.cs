using System;
using System.Collections.Generic;
using System.Linq;

namespace wi_crawler
{
    public class TfIdfIndex : Indexer
    {
        private readonly List<TermFrequencyVector> _termFrequencyVectors = new List<TermFrequencyVector>();

        public void BuildTfIdfIndex()
        {
            using var db = new CrawlingContext();
            List<Webpage> webpages = db.Webpages.OrderBy(x => x.WebpageId).ToList();

            foreach (Webpage webpage in webpages)
            {
                AddWebpageToTfIdfIndex(webpage);
            }
        }

        private void AddWebpageToTfIdfIndex(Webpage webpage)
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
            var termFrequencyVector = new TermFrequencyVector(term.Key, documentFrequencies);
            _termFrequencyVectors.Add(termFrequencyVector);
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
    }
}