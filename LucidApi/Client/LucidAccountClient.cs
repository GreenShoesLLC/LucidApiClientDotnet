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
    public class LucidAccountClient : LucidClientBase
    {
        public override string AuthUrl => "https://lucid.app/oauth2/authorizeAccount";
        private static readonly HashSet<string> AccountOnlyScopes = new HashSet<string>
        {
            "account.info",
            "account.user",
            "account.user:readonly",
            "account.user.transfercontent"
        };

        public LucidAccountClient(string clientId, string clientSecret, string redirectUri, List<string> scopes, bool isPublished = false) : base(clientId, clientSecret, redirectUri, scopes, isPublished)
        {

        }

        public async IAsyncEnumerable<JToken> ManageUsersAsync(string action, Dictionary<string, object>? userData = null, string apiVersion = "1", int pageSize = 200)
        {
            if (!_scopes.Contains("account.user") && !_scopes.Contains("account.user:readonly"))
            {
                throw new LucidOAuthException("invalid_scopes", "Insufficient scope for managing users");
            }

            var url = $"{ApiBaseUrl}/users";

            switch (action)
            {
                case "view":
                    await foreach (var page in MakePaginatedRequestAsync("GET", url, apiVersion: apiVersion, pageSize: pageSize))
                    {
                        yield return page;
                    }
                    break;
                case "create":
                    if (_scopes.Contains("account.user:readonly"))
                    {
                        throw new LucidOAuthException("invalid_scopes", "Readonly scope insufficient for creating users");
                    }
                    yield return await MakeApiRequestAsync("POST", url, userData, apiVersion);
                    break;
                case "update":
                    if (_scopes.Contains("account.user:readonly"))
                    {
                        throw new LucidOAuthException("invalid_scopes", "Readonly scope insufficient for updating users");
                    }
                    yield return await MakeApiRequestAsync("PUT", url, userData, apiVersion);
                    break;
                case "delete":
                    if (_scopes.Contains("account.user:readonly"))
                    {
                        throw new LucidOAuthException("invalid_scopes", "Readonly scope insufficient for deleting users");
                    }
                    yield return await MakeApiRequestAsync("DELETE", url, userData, apiVersion);
                    break;
                default:
                    throw new ArgumentException("Invalid action. Must be 'create', 'update', 'delete', or 'view'");
            }
        }
        public async Task<JToken> GetUserAsync(string userId, string apiVersion = "1")
        {
            var url = $"{ApiBaseUrl}/users/{userId}";
            return await MakeApiRequestAsync("GET", url, apiVersion: apiVersion);
        }
        public async Task<List<JToken>> GetUsersAsync(string apiVersion = "1")
        {
            var url = $"{ApiBaseUrl}/users";
            try
            {
                var response = await MakeApiRequestAsync("GET", url, apiVersion: apiVersion);

                if (response == null)
                {
                    throw new InvalidOperationException("API request returned null response.");
                }

                var usersToken = response["users"];
                if (usersToken == null || usersToken.Type == JTokenType.Null)
                {
                    // If "users" key is missing or null, return an empty list
                    return new List<JToken>();
                }

                if (usersToken.Type != JTokenType.Array)
                {
                    throw new JsonException("The 'users' property is not an array as expected.");
                }

                return usersToken.ToObject<List<JToken>>() ?? new List<JToken>();
            }
            catch (JsonException ex)
            {
                // Handle JSON parsing errors
                Console.WriteLine($"Error parsing JSON response: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"Error occurred while fetching users: {ex.Message}");
                throw;
            }
        }
    }
}
