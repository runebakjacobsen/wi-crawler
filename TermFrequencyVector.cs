using System.Collections.Generic;

namespace wi_crawler
{
    public class TermFrequencyVector
    {
        public TermFrequencyVector(string term, List<DocumentFrequency> documentFrequencies)
        {
            Term = term;
            DocumentFrequencies = documentFrequencies;
        }

        public string Term { get; set; }

        public List<DocumentFrequency> DocumentFrequencies { get; set; }
    }
}