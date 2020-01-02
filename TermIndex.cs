using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace wi_crawler
{
    public class TermIndex
    {
        [Key]
        public string Term { get; set; }
        public LinkedList<int> WebpageIds { get; } = new LinkedList<int>();
    }
}