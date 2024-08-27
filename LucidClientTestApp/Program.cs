// See https://aka.ms/new-console-template for more information
using dotenv.net;
using Newtonsoft.Json;
using LucidApi.Client;
using LucidApi.Models;


class Program
{
    private static readonly string? lucidClientId;
    private static readonly string? lucidClientSecret;
    private static readonly string? lucidRedirectUri;

    static Program()
    {
        Console.WriteLine("Current Directory: " + Directory.GetCurrentDirectory());
        string envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        Console.WriteLine(".env file path: " + envPath);
        Console.WriteLine(".env file exists: " + File.Exists(envPath));

        if (File.Exists(envPath))
        {
            Console.WriteLine(".env file contents:");
            Console.WriteLine(File.ReadAllText(envPath));
        }

        try
        {
            DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { envPath }));
            Console.WriteLine(".env file loaded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading .env file: " + ex.Message);
        }

        lucidClientId = GetEnvironmentVariable("LUCID_CLIENT_ID");
        lucidClientSecret = GetEnvironmentVariable("LUCID_CLIENT_SECRET");
        lucidRedirectUri = GetEnvironmentVariable("LUCID_REDIRECT_URI");

        Console.WriteLine("LUCID_CLIENT_ID: " + (lucidClientId ?? "not found"));
        Console.WriteLine("LUCID_CLIENT_SECRET: " + (lucidClientSecret != null ? "found" : "not found"));
        Console.WriteLine("LUCID_REDIRECT_URI: " + (lucidRedirectUri ?? "not found"));
    }
    static async Task Main(string[] args)
    {
        // await AccountClientStuff();
        await UserClientStuff();


    }
    private static string? GetEnvironmentVariable(string variableName)
    {
        string? value = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrWhiteSpace(value))
        {
            Console.WriteLine($"Warning: Environment variable '{variableName}' is not set or is empty.");
            return null;
        }
        return value;
    }
    static async Task AccountClientStuff()
    {
        if (!string.IsNullOrEmpty(lucidClientId) && !string.IsNullOrEmpty(lucidClientSecret) && !string.IsNullOrEmpty(lucidRedirectUri))
        {
            var accountClient = new LucidAccountClient(lucidClientId, lucidClientSecret, lucidRedirectUri, LucidScopes.AccountScopes);
            await GetAccessTokenAsync(accountClient);

            try
            {
                if (accountClient.TokenData != null && accountClient.TokenData.TryGetValue("access_token", out var accessToken) &&
                    accessToken != null)
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    var introspectionResult = await accountClient.IntrospectTokenAsync(accessToken.ToString());
#pragma warning restore CS8604 // Possible null reference argument.
                    Console.WriteLine($"Introspection result: {JsonConvert.SerializeObject(introspectionResult, Formatting.Indented)}");

                    var accountInfo = await accountClient.GetAccountInfoAsync();
                    Console.WriteLine($"User profile: {accountInfo}");
                }
                else
                {
                    // Handle the case where the access token is not available
                    Console.WriteLine("Access token is not available.");
                    // Or throw an exception, log an error, etc.
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during token introspection: {ex.Message}");
            }
        }





    }
    static async Task UserClientStuff()
    {
        if (!string.IsNullOrEmpty(lucidClientId) && !string.IsNullOrEmpty(lucidClientSecret) && !string.IsNullOrEmpty(lucidRedirectUri))
        {
            var user_client = new LucidUserClient(lucidClientId, lucidClientSecret, lucidRedirectUri, LucidScopes.UserScopes);
            await GetAccessTokenAsync(user_client);


            try
            {

                if (user_client.TokenData != null && user_client.TokenData.TryGetValue("access_token", out var accessToken) && accessToken != null)
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    var introspectionResult = await user_client.IntrospectTokenAsync(accessToken.ToString());
#pragma warning restore CS8604 // Possible null reference argument.

                    Console.WriteLine($"Introspection result: {JsonConvert.SerializeObject(introspectionResult, Formatting.Indented)}");

                    var accountInfo = await user_client.GetAccountInfoAsync();
                    Console.WriteLine($"User profile: {accountInfo}");

                    // Fetch all documents without any filters
                    var allDocuments = await user_client.SearchDocumentsAsync();

                    foreach (var document in allDocuments.Where(x => x.Title == "FlowchartQuodsi"))
                    {
                        Console.WriteLine($"Document ID: {document.DocumentId ?? "N/A"}, Title: {document.Title ?? "Untitled"}");

                        if (string.IsNullOrEmpty(document.DocumentId))
                        {
                            Console.WriteLine("Skipping document with null or empty ID");
                            continue;
                        }

                        var documentContents = await user_client.GetDocumentContentsAsync(document.DocumentId);

                        Console.WriteLine($"Document Title: {documentContents.Title ?? "Untitled"}");
                        foreach (var page in documentContents.Pages ?? Enumerable.Empty<Page>())
                        {
                            Console.WriteLine($"Page Title: {page.Title ?? "Untitled"}");

                            foreach (var dataPair in page.CustomData ?? Enumerable.Empty<DataPair>())
                            {
                                Console.WriteLine($"Page Data: {dataPair.Key ?? "N/A"} {dataPair.Value ?? "N/A"}");
                            }

                            foreach (var shape in page.Items?.Shapes ?? Enumerable.Empty<Shape>())
                            {
                                Console.WriteLine($"Shape ID: {shape.Id ?? "N/A"}, Class: {shape.Class ?? "N/A"}");
                                foreach (var item in shape.CustomData ?? Enumerable.Empty<DataPair>())
                                {
                                    Console.WriteLine($"Shape Data: {item.Key ?? "N/A"} {item.Value ?? "N/A"}");
                                }
                            }

                            foreach (var line in page.Items?.Lines ?? Enumerable.Empty<Line>())
                            {
                                Console.WriteLine($"Line ID: {line.Id ?? "N/A"}, Endpoint1: {line.Endpoint1?.ToString() ?? "N/A"}, Endpoint2: {line.Endpoint2?.ToString() ?? "N/A"}");
                                foreach (var item in line.CustomData ?? Enumerable.Empty<DataPair>())
                                {
                                    Console.WriteLine($"Line Data: {item.Key ?? "N/A"} {item.Value ?? "N/A"}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Handle the case where the access token is not available
                    Console.WriteLine("Access token is not available.");
                    // Or throw an exception, log an error, etc.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during token introspection: {ex.Message}");
            }
        }

    }

    static async Task<Dictionary<string, object>> GetAccessTokenAsync(LucidClientBase client)
    {
        var auth_url = client.GetAuthorizationUrl();
        Console.WriteLine($"Please visit this URL to authorize the application: {auth_url}");
        // add code here to request the user enter the code from the link

        Console.Write("Enter the authorization code: ");
        string? code = Console.ReadLine();
        if (code == null)
        {
            throw new InvalidOperationException("Authorization code cannot be null.");
        }

        var token_data = await client.GetAccessTokenAsync(code);
        Console.Write("Access token obtained and stored.");
        return token_data;
    }

}