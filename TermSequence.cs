namespace wi_crawler
{
    public class TermSequence
    {
        public TermSequence(string term, Webpage webpage)
        {
            Term = term;
            Webpage = webpage;
        }
        public string Term { get; set; }
        public Webpage Webpage { get; set; }
    }
}