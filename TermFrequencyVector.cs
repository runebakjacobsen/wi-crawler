using System.Collections.Generic;

namespace wi_crawler
{
    public class TermFrequencyVector
    {
        public string Term { get; set; }

        public List<DocumentFrequency> DocumentFrequencies { get; set; }
    }
}