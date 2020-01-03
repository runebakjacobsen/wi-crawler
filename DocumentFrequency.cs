namespace wi_crawler
{
    public class DocumentFrequency
    {
        public DocumentFrequency(int webpageId, double termWeight)
        {
            WebpageId = webpageId;
            TermWeight = termWeight;
        }

        public int WebpageId { get; set; }
        public double TermWeight { get; set; }
    }
}