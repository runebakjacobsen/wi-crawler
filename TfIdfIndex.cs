using System;
using System.Collections.Generic;
using System.Linq;

namespace wi_crawler
{
    public class TfIdfIndex : Indexer
    {
        private readonly List<TermFrequencyVector> _termFrequencyVectors = new List<TermFrequencyVector>();


        private static double[][] TransformToTFIDFVectors(List<List<string>> stemmedDocs, Dictionary<string, double> vocabularyIDF)
        {
            // Transform each document into a vector of tfidf values.
            List<List<double>> vectors = new List<List<double>>();
            foreach (var doc in stemmedDocs)
            {
                List<double> vector = new List<double>();
                foreach (var vocab in vocabularyIDF)
                {
                    // Term frequency = count how many times the term appears in this document.
                    double tf = doc.Where(d => d == vocab.Key).Count();
                    double tfidf = tf * vocab.Value;
                    double tfidf = TfIdfWeighting(tf, ste)
                    vector.Add(tfidf);
                }
                vectors.Add(vector);
            }
            return vectors.Select(v => v.ToArray()).ToArray();
        }

        public void CosineScore(string query)
        {
            List<double> scores = new List<double>();
            List<double> length = new List<double>();

            List<string> querySplit = query.Split(" ").ToList();

            foreach (var term in querySplit)
            {
                var wtq = TfIdfWeighting(term, querySplit.Count());
                var postingsList = _termFrequencyVectors.FindAll(x => x.Term.Equals(term));
                foreach (var item in postingsList)
                {

                    scores.Add(item.DocumentFrequencies.)
                }
            }
        }
        private Dictionary<int, List<string>> _stemmedDocs;
        public void BuildTfIdfIndex()
        {
            _stemmedDocs = GenerateStemmedDocuments();

            using var db = new CrawlingContext();
            List<Webpage> webpages = db.Webpages.OrderBy(x => x.WebpageId).ToList();

            foreach (Webpage webpage in webpages)
            {
                AddWebpageToTfIdfIndex(webpage);
            }
        }

        private void AddWebpageToTfIdfIndex(Webpage webpage)
        {
            var stemmedContent = _stemmedDocs[webpage.WebpageId];

            Dictionary<string, int> termFrequencyLookup = CountTermFrequencies(stemmedContent);

            foreach (var term in termFrequencyLookup)
            {
                var weighting = TfIdfWeighting(term.Value, GetDocumentFrequency(term.Key));
                var documentFrequency = new DocumentFrequency(webpage.WebpageId, weighting);
                AddOrUpdateTermFrequencyVectors(term, documentFrequency);
            }
        }

        private int GetDocumentFrequency(string term)
        {
            int occurrence = 0;
            foreach (var item in _stemmedDocs)
            {
                if (item.Value.Contains(term))
                {
                    occurrence++;
                }
            }

            return occurrence;
        }

        private Dictionary<int, List<string>> GenerateStemmedDocuments()
        {
            using var db = new CrawlingContext();

            var webpages = db.Webpages.ToList();

            var stemmedDocs = new Dictionary<int, List<string>>();

            foreach (var webpage in webpages)
            {
                var content = RemoveStopWords(webpage.Content);
                var stemmedContent = Stemmer(content);
                stemmedDocs.Add(webpage.WebpageId, stemmedContent);
            }

            return stemmedDocs;
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

        private double TfIdfWeighting(double termFrequency, int numberOfDocsContainingTerm)
        {
            return LogTermFrequency(termFrequency) * InverseDocumentFrequency(termFrequency, numberOfDocsContainingTerm);
        }

        private double LogTermFrequency(double termFrequency)
        {
            if (termFrequency > 0)
            {
                return 1 + Math.Log10(termFrequency);
            }

            return 0;
        }

        private double InverseDocumentFrequency(double termFrequency, int numberOfDocsContainingTerm)
        {
            return Math.Log10(numberOfDocsContainingTerm / termFrequency);
        }
    }
}