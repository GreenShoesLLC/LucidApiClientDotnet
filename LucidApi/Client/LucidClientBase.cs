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
    public abstract class LucidClientBase
    {
        protected string _clientId;
        protected string _clientSecret;
        protected string _redirectUri;
        protected List<string> _scopes;
        protected Dictionary<string, object> _tokenData;
        protected bool _isPublished;

        public abstract string AuthUrl { get; }


        protected const string TokenUrl = "https://api.lucid.co/oauth2/token";
        protected const string ApiBaseUrl = "https://api.lucid.co";

        public Dictionary<string, object> TokenData
        {
            get { return _tokenData; }
            private set { _tokenData = value; }
        }

        private static readonly HashSet<string> AccountOnlyScopes = new HashSet<string>
        {
            "account.info",
            "account.user",
            "account.user:readonly",
            "account.user.transfercontent"
        };

        private static readonly HashSet<string> UserOnlyScopes = new HashSet<string>
        {
            "user.profile"
        };

        public LucidClientBase(string clientId, string clientSecret, string redirectUri, List<string> scopes, bool isPublished = false)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;
            _scopes = scopes;
            _isPublished = isPublished;

        }

        public string GetAuthorizationUrl()
        {
            if (!_isPublished)
            {
                Console.WriteLine("Warning: This app is not published. Only specified app collaborators can use it.");
            }

            var query = new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "redirect_uri", _redirectUri },
                { "scope", string.Join(" ", _scopes) },
                { "response_type", "code" }
            };

            var queryString = string.Join("&", query.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

            return $"{AuthUrl}?{queryString}";
        }
        public async Task<Dictionary<string, object>> GetAccessTokenAsync(string code)
        {
            var data = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "authorization_code" },
                { "redirect_uri", _redirectUri }
            };

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsync(TokenUrl, new FormUrlEncodedContent(data));
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        _tokenData = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                        _tokenData["expires_at"] = DateTime.Now.AddSeconds(Convert.ToInt32(_tokenData["expires_in"]));
                        return _tokenData;
                    }
                    else
                    {
                        var errorData = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                        throw new LucidOAuthException(
                            errorData.GetValueOrDefault("error", "unknown_error").ToString(),
                            errorData.GetValueOrDefault("error_description", "Unknown error occurred").ToString()
                        );
                    }
                }
                catch (HttpRequestException e)
                {
                    throw new LucidOAuthException("network_error", $"Error occurred while making the request: {e.Message}");
                }
                catch (JsonException e)
                {
                    throw new LucidOAuthException("parsing_error", $"Error occurred while parsing the response: {e.Message}");
                }
            }
        }

        public async Task<Dictionary<string, object>> RefreshTokenAsync()
        {
            if (_tokenData == null || !_tokenData.ContainsKey("refresh_token"))
            {
                throw new LucidOAuthException("invalid_grant", "No refresh token available");
            }

            var data = new Dictionary<string, string>
            {
                { "refresh_token", _tokenData["refresh_token"].ToString() },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "refresh_token" }
            };

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(TokenUrl, new FormUrlEncodedContent(data));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _tokenData = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                    _tokenData["expires_at"] = DateTime.Now.AddSeconds(Convert.ToInt32(_tokenData["expires_in"]));
                    return _tokenData;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorData = JsonConvert.DeserializeObject<Dictionary<string, object>>(errorContent);
                    throw new LucidOAuthException(errorData.GetValueOrDefault("error", "unknown_error").ToString(), errorData.GetValueOrDefault("error_description", "Unknown error occurred").ToString());
                }
            }
        }

        public async Task<Dictionary<string, object>> IntrospectTokenAsync(string token)
        {
            var url = $"{ApiBaseUrl}/oauth2/token/introspect";
            var data = new Dictionary<string, object>
            {
                { "token", token },
                { "client_id", _clientId },
                { "client_secret", _clientSecret }
            };

            try
            {
                var response = await MakeApiRequestAsync("POST", url, data);
                return response.ToObject<Dictionary<string, object>>();
            }
            catch (LucidOAuthException ex)
            {
                if (ex.Message.Contains("403"))
                {
                    throw new LucidOAuthException("invalid_client", "Invalid client credentials");
                }
                throw;
            }
        }

        public async Task<bool> IsTokenValidAsync()
        {
            if (_tokenData == null || !_tokenData.ContainsKey("access_token"))
            {
                return false;
            }

            try
            {
                var introspectionResult = await IntrospectTokenAsync(_tokenData["access_token"].ToString());

                // Check if the 'active' key exists and is a boolean
                if (introspectionResult.TryGetValue("active", out object activeValue) && activeValue is bool active)
                {
                    return active;
                }

                // If 'active' is not a boolean or doesn't exist, assume the token is invalid
                return false;
            }
            catch (LucidOAuthException)
            {
                // If an exception occurs during introspection, consider the token invalid
                return false;
            }
            catch (Exception ex)
            {
                // Log unexpected exceptions, but still return false
                Console.WriteLine($"Unexpected error in IsTokenValidAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetValidAccessTokenAsync()
        {
            if (_tokenData == null)
            {
                throw new LucidOAuthException("invalid_grant", "No token data available. Call GetAccessTokenAsync first.");
            }

            // Check if the token has expired based on the expires_at field
            if (_tokenData.TryGetValue("expires_at", out object expiresAtObj) &&
                expiresAtObj is DateTime expiresAt &&
                expiresAt <= DateTime.Now.AddMinutes(5)) // Add a 5-minute buffer
            {
                await RefreshTokenAsync();
            }

            if (!_tokenData.ContainsKey("access_token") || string.IsNullOrEmpty(_tokenData["access_token"].ToString()))
            {
                throw new LucidOAuthException("invalid_token", "Access token is missing or empty.");
            }

            var accessToken = _tokenData["access_token"].ToString();
            Console.WriteLine($"Using access token: {accessToken.Substring(0, Math.Min(10, accessToken.Length))}..."); // Print up to the first 10 characters
            return accessToken;
        }

        public async Task<JToken> MakeApiRequestAsync(string method, string url, Dictionary<string, object> data = null, string apiVersion = "1")
        {
            var accessToken = await GetValidAccessTokenAsync();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                client.DefaultRequestHeaders.Add("Lucid-Api-Version", apiVersion);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                Console.WriteLine($"Attempting to access URL: {url}");
                Console.WriteLine($"HTTP Method: {method}");
                Console.WriteLine("Request Headers:");
                foreach (var header in client.DefaultRequestHeaders)
                {
                    Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
                }
                HttpResponseMessage response;

                switch (method.ToUpper())
                {
                    case "GET":
                        response = await client.GetAsync(url);
                        break;
                    case "POST":
                        var content = new StringContent(JsonConvert.SerializeObject(data), System.Text.Encoding.UTF8, "application/json");
                        response = await client.PostAsync(url, content);
                        break;
                    case "PUT":
                        content = new StringContent(JsonConvert.SerializeObject(data), System.Text.Encoding.UTF8, "application/json");
                        response = await client.PutAsync(url, content);
                        break;
                    case "DELETE":
                        response = await client.DeleteAsync(url);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported HTTP method: {method}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Status: {response.StatusCode}");
                Console.WriteLine($"Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(responseContent))
                    {
                        return null;
                    }

                    try
                    {
                        return JToken.Parse(responseContent);
                    }
                    catch (JsonReaderException)
                    {
                        Console.WriteLine("Failed to parse response as JSON. Returning raw content.");
                        return JToken.FromObject(responseContent);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new LucidOAuthException("invalid_client", "An admin on the Lucid account may have disabled access to your OAuth2 client.");
                }
                else
                {
                    throw new LucidOAuthException("api_error", $"API request failed with status code {response.StatusCode}. Response: {responseContent}");
                }
            }
        }

        protected async Task<JToken> MakeSingleApiRequestAsync(string method, string url, Dictionary<string, object> data = null, string apiVersion = "1")
        {
            try
            {
                return await MakeApiRequestAsync(method, url, data, apiVersion);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in API request: {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// Makes a paginated request to the API and returns a list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to which the JSON objects should be converted.</typeparam>
        /// <param name="method">The HTTP method (GET, POST, etc.)</param>
        /// <param name="url">The URL of the API endpoint.</param>
        /// <param name="data">Optional data to be sent with the request (for POST, PUT, etc.).</param>
        /// <param name="parameters">Optional query parameters for the request.</param>
        /// <param name="apiVersion">The API version to use.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="tokenSelector">A function to select the token containing the items from the paginated response.</param>
        /// <returns>A list of items of type T.</returns>
        protected async Task<List<T>> MakePaginatedRequestAsync<T>(string method, string url, Dictionary<string, object> data = null, Dictionary<string, object> parameters = null, string apiVersion = "1", int pageSize = 200, Func<JToken, JToken> tokenSelector = null)
        {
            var results = new List<T>();

            // Set up the parameters
            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }
            parameters["pageSize"] = Math.Min(pageSize, 200);

            // Handle pagination
            while (true)
            {
                var response = await MakeSingleApiRequestAsync(method, url, data, apiVersion);
                if (response == null)
                {
                    break;
                }

                // Select the token containing the items, if provided
                var itemsToken = tokenSelector != null ? tokenSelector(response) : response;
                if (itemsToken != null)
                {
                    var items = itemsToken.ToObject<List<T>>();
                    if (items != null)
                    {
                        results.AddRange(items);
                    }
                }

                // Get the next URL from the Link header
                var linkHeader = response.SelectToken("$.headers.Link");
                if (linkHeader == null)
                {
                    break;
                }

                string nextUrl = null;
                foreach (var link in linkHeader.ToString().Split(','))
                {
                    if (link.Contains("rel=\"next\""))
                    {
                        nextUrl = link.Split(';')[0].Trim('<', '>');
                        break;
                    }
                }

                if (nextUrl == null)
                {
                    break;
                }

                url = nextUrl;
                parameters.Clear();
            }

            return results;
        }

        /// <summary>
        /// Asynchronously makes a paginated API request to the Lucid API.
        /// This method will handle fetching multiple pages of data until all records are retrieved.
        /// </summary>
        /// <param name="method">The HTTP method (GET, POST, PUT, DELETE) to use for the request.</param>
        /// <param name="url">The initial URL for the API endpoint.</param>
        /// <param name="data">Optional data to be sent with the request. Typically used with POST or PUT requests.</param>
        /// <param name="parameters">Optional query parameters to include in the request URL. If null, a new dictionary will be created.</param>
        /// <param name="apiVersion">The API version to use. Defaults to "1".</param>
        /// <param name="pageSize">The number of records to return per page. Defaults to 200, which is the maximum allowed by the API.</param>
        /// <returns>An async enumerable of JToken representing the paginated responses.</returns>
        protected async IAsyncEnumerable<JToken> MakePaginatedRequestAsync(string method, string url, Dictionary<string, object> data = null, Dictionary<string, object> parameters = null, string apiVersion = "1", int pageSize = 200)
        {
            // Ensure the parameters dictionary is not null
            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }

            // Set the pageSize parameter, ensuring it does not exceed the maximum allowed value of 200
            parameters["pageSize"] = Math.Min(pageSize, 200);

            // Continuously fetch pages until there are no more pages to fetch
            while (true)
            {
                // Make a single API request and get the response
                var response = await MakeSingleApiRequestAsync(method, url, data, apiVersion);
                if (response == null)
                {
                    // If the response is null, end the iteration
                    yield break;
                }

                // Yield the current response to the caller
                yield return response;

                // Extract the 'Link' header from the response, which contains pagination information
                var linkHeader = response.SelectToken("$.headers.Link");
                if (linkHeader == null)
                {
                    // If there is no 'Link' header, it means there are no more pages to fetch
                    break;
                }

                // Parse the 'Link' header to find the URL for the next page
                string nextUrl = null;
                foreach (var link in linkHeader.ToString().Split(','))
                {
                    if (link.Contains("rel=\"next\""))
                    {
                        // Extract the URL for the next page from the 'Link' header
                        nextUrl = link.Split(';')[0].Trim('<', '>');
                        break;
                    }
                }

                // If no next URL is found, it means we have reached the last page
                if (nextUrl == null)
                {
                    break;
                }

                // Update the URL to the next page's URL and clear the parameters for the next request
                url = nextUrl;
                parameters.Clear();
            }
        }


        public List<CustomAttribute> ParseCustomAttributes(JArray attributes)
        {
            var parsedAttributes = new List<CustomAttribute>();

            foreach (var attr in attributes)
            {
                var attrType = Enum.Parse<AttributeType>(attr["type"].ToString());
                var name = attr["name"]?.ToString();
                var value = attr["value"]?.ToObject<object>();

                if (attrType == AttributeType.HierarchicalDropdown && value != null)
                {
                    value = ((JArray)value).Select(item => new HierarchicalDropdownValue
                    {
                        Name = item["name"].ToString(),
                        Value = item["value"].ToString()
                    }).ToList();
                }

                parsedAttributes.Add(new CustomAttribute
                {
                    Type = attrType,
                    Name = name,
                    Value = value
                });
            }

            return parsedAttributes;
        }
        public async Task<AccountInfo> GetAccountInfoAsync(string apiVersion = "1")
        {
            var url = $"{ApiBaseUrl}/accounts/me";
            try
            {
                var response = await MakeApiRequestAsync("GET", url, apiVersion: apiVersion);
                return response.ToObject<AccountInfo>();
            }
            catch (Exception ex)
            {
                throw new LucidOAuthException("api_error", $"Failed to get account info: {ex.Message}");
            }
        }
    }
}