using System;
using System.Collections.Generic;

namespace LucidApi.Models
{
    public class Document
    {
        public string DocumentId { get; set; }
        public string Title { get; set; }
        public string EditUrl { get; set; }
        public string ViewUrl { get; set; }
        public int Version { get; set; }
        public int PageCount { get; set; }
        public bool CanEdit { get; set; }
        public DateTime Created { get; set; }
        public int CreatorId { get; set; }
        public DateTime LastModified { get; set; }
        public int LastModifiedUserId { get; set; }
        public List<CustomAttribute> CustomAttributes { get; set; }
        public List<string> CustomTags { get; set; }
        public Product Product { get; set; }
        public string Status { get; set; }
        public DateTime? Trashed { get; set; }
        public int? Parent { get; set; }
        public DocumentOwner Owner { get; set; }
    }
}