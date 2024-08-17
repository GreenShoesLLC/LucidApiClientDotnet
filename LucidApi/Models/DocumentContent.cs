using System.Collections.Generic;

namespace LucidApi.Models
{
    public class DocumentContent
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public Product Product { get; set; }
        public List<Page> Pages { get; set; }
        public Data Data { get; set; }
    }
}