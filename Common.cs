namespace wi_crawler
{
    public class Common
    {
        public static string NormalizeHost(string host)
        {
            if (host.StartsWith("www."))
            {
                return host.Remove(0, 4);
            }
            return host;
        }
    }
}