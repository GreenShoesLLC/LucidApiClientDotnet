using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LucidApi.Models
{
    public class Document
    {
        [JsonPropertyName("documentId")]
        public string? DocumentId { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("editUrl")]
        public string? EditUrl { get; set; }

        [JsonPropertyName("viewUrl")]
        public string? ViewUrl { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("pageCount")]
        public int PageCount { get; set; }

        [JsonPropertyName("canEdit")]
        public bool CanEdit { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("creatorId")]
        public int CreatorId { get; set; }

        [JsonPropertyName("lastModified")]
        public DateTime LastModified { get; set; }

        [JsonPropertyName("lastModifiedUserId")]
        public int LastModifiedUserId { get; set; }

        [JsonPropertyName("customAttributes")]
        public List<CustomAttribute>? CustomAttributes { get; set; }

        [JsonPropertyName("customTags")]
        public List<string>? CustomTags { get; set; }

        [JsonPropertyName("product")]
        public Product? Product { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("trashed")]
        public DateTime? Trashed { get; set; }

        [JsonPropertyName("parent")]
        public int? Parent { get; set; }

        [JsonPropertyName("owner")]
        public DocumentOwner? Owner { get; set; }
    }
}