namespace wi_crawler
{
    public class DocumentFrequency
    {
        public DocumentFrequency(int webpageId, double termFrequency)
        {
            WebpageId = webpageId;
            TermFrequency = termFrequency;
        }

        public int WebpageId { get; set; }
        public double TermFrequency { get; set; }
    }
}