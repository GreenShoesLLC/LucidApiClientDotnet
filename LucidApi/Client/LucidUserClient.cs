using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LucidApi.Models;
using LucidApi.Exceptions;

namespace LucidApi.Client
{
    public class LucidUserClient : LucidClientBase
    {
        public override string AuthUrl => "https://lucid.app/oauth2/authorize";
        private static readonly HashSet<string> UserOnlyScopes = new HashSet<string>
        {
            "user.profile"
        };

        public LucidUserClient(string clientId, string clientSecret, string redirectUri, List<string> scopes, bool isPublished = false) : base(clientId, clientSecret, redirectUri, scopes, isPublished)
        {

        }
        public async Task<List<Document>> GetLucidDocumentsAsync(string apiVersion = "1", int pageSize = 200)
        {
            if (!_scopes.Contains("lucidchart.document.content") && !_scopes.Contains("lucidchart.document.content:readonly"))
            {
                throw new LucidOAuthException("invalid_scopes", "Insufficient scope for accessing Lucidchart documents");
            }

            var url = $"{ApiBaseUrl}/documents";
            var parameters = new Dictionary<string, object>
            {
                { "pageSize", pageSize },
                { "fields", "id,title,status,type,owner" }
            };

            try
            {
                var result = await MakePaginatedRequestAsync<Document>(
                    "GET",
                    url,
                    parameters: parameters,
                    apiVersion: apiVersion,
                    pageSize: pageSize,
                    tokenSelector: response => response["documents"] ?? JToken.FromObject(new List<Document>())
                ) ?? new List<Document>();

                return result ?? new List<Document>();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error fetching Lucid documents: {ex.Message}");
                // Return an empty list instead of null
                return new List<Document>();
            }
        }

        public async Task<List<Document>> SearchDocumentsAsync(DocumentSearchParameters? parameters = null, string apiVersion = "1")
        {
            var url = $"{ApiBaseUrl}/documents/search";
            parameters ??= new DocumentSearchParameters();

            // Truncate keywords to 400 characters if provided
            if (!string.IsNullOrEmpty(parameters.Keywords))
            {
                parameters.Keywords = parameters.Keywords.Substring(0, Math.Min(parameters.Keywords.Length, 400));
            }

            var data = new Dictionary<string, object>();

            if (parameters.Product != null && parameters.Product.Any())
            {
                data["product"] = parameters.Product;
            }

            if (parameters.CreatedStartTime.HasValue)
            {
                data["createdStartTime"] = parameters.CreatedStartTime.Value.ToString("o");
            }

            if (parameters.CreatedEndTime.HasValue)
            {
                data["createdEndTime"] = parameters.CreatedEndTime.Value.ToString("o");
            }

            if (parameters.LastModifiedAfter.HasValue)
            {
                data["lastModifiedAfter"] = parameters.LastModifiedAfter.Value.ToString("o");
            }

            if (!string.IsNullOrEmpty(parameters.Keywords))
            {
                data["keywords"] = parameters.Keywords;
            }

            // Remove null values
            foreach (var key in data.Keys.ToList())
            {
                if (data[key] == null)
                {
                    data.Remove(key);
                }
            }

            // This selector function will handle different response structures
            Func<JToken, JToken> tokenSelector = response =>
            {
                if (response.Type == JTokenType.Array)
                {
                    // Handle case where response is an array of documents
                    return response;
                }
                else if (response.Type == JTokenType.Object)
                {
                    // Handle case where response is a single document
                    return new JArray(response);
                }
                else
                {
                    throw new InvalidOperationException("Unexpected response format");
                }
            };

            return await MakePaginatedRequestAsync<Document>("POST", url, data: data, apiVersion: apiVersion, tokenSelector: tokenSelector);
        }

        /// <summary>
        /// Retrieves the contents of a specific Lucidchart document.
        /// </summary>
        /// <param name="documentId">The ID of the document to retrieve contents for.</param>
        /// <param name="apiVersion">The API version to use. Defaults to "1".</param>
        /// <returns>A DocumentContent object representing the contents of the document.</returns>
        /// <exception cref="LucidOAuthException">Thrown when the client lacks the required scopes.</exception>
        /// <exception cref="ArgumentNullException">Thrown when documentId is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
        /// <exception cref="JsonException">Thrown when the response cannot be deserialized.</exception>
        public async Task<DocumentContent> GetDocumentContentsAsync(string documentId, string apiVersion = "1")
        {
            if (string.IsNullOrEmpty(documentId))
            {
                throw new ArgumentNullException(nameof(documentId), "Document ID cannot be null or empty.");
            }

            if (!_scopes.Contains("lucidchart.document.content") && !_scopes.Contains("lucidchart.document.content:readonly"))
            {
                throw new LucidOAuthException("invalid_scopes", "Insufficient scope for accessing Lucidchart document contents");
            }

            var url = $"{ApiBaseUrl}/documents/{documentId}/contents";

            try
            {
                var response = await MakeApiRequestAsync("GET", url, apiVersion: apiVersion);

                if (response == null)
                {
                    throw new HttpRequestException("API request returned null response.");
                }

                var documentContent = response.ToObject<DocumentContent>();

                if (documentContent == null)
                {
                    throw new JsonException("Failed to deserialize the API response to DocumentContent.");
                }

                return documentContent;
            }
            catch (HttpRequestException ex)
            {
                // Log the exception
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                throw; // Re-throw the exception
            }
            catch (JsonException ex)
            {
                // Log the exception
                Console.WriteLine($"JSON deserialization failed: {ex.Message}");
                throw; // Re-throw the exception
            }
        }
    }
}