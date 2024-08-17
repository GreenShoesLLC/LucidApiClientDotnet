// See https://aka.ms/new-console-template for more information
using dotenv.net;
using Newtonsoft.Json;
using LucidApi.Client;
using LucidApi.Models;


class Program
{
    private static readonly string? clientId;
    private static readonly string? clientSecret;
    private static readonly string? redirectUri;

    private static readonly List<string> userScopes = new List<string>
    {
        "lucidchart.document.content",
        "lucidchart.document.content:readonly",
        "offline_access",
        "user.profile",
        "account.user:readonly",
        "account.info"
    };

    private static readonly List<string> accountScopes = new List<string>
    {
        "offline_access",
        "account.user:readonly",
        "account.info"
    };
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

        clientId = GetEnvironmentVariable("CLIENT_ID");
        clientSecret = GetEnvironmentVariable("CLIENT_SECRET");
        redirectUri = GetEnvironmentVariable("REDIRECT_URI");

        Console.WriteLine("CLIENT_ID: " + (clientId ?? "not found"));
        Console.WriteLine("CLIENT_SECRET: " + (clientSecret != null ? "found" : "not found"));
        Console.WriteLine("REDIRECT_URI: " + (redirectUri ?? "not found"));
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
        var accountClient = new LucidAccountClient(clientId, clientSecret, redirectUri, accountScopes);
        await GetAccessTokenAsync(accountClient);


        try
        {
            var introspectionResult = await accountClient.IntrospectTokenAsync(accountClient.TokenData["access_token"].ToString());
            Console.WriteLine($"Introspection result: {JsonConvert.SerializeObject(introspectionResult, Formatting.Indented)}");

            var accountInfo = await accountClient.GetAccountInfoAsync();
            Console.WriteLine($"User profile: {accountInfo}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during token introspection: {ex.Message}");
        }

    }
    static async Task UserClientStuff()
    {
        var user_client = new LucidUserClient(clientId, clientSecret, redirectUri, userScopes);
        await GetAccessTokenAsync(user_client);


        try
        {
            var introspectionResult = await user_client.IntrospectTokenAsync(user_client.TokenData["access_token"].ToString());
            Console.WriteLine($"Introspection result: {JsonConvert.SerializeObject(introspectionResult, Formatting.Indented)}");

            var accountInfo = await user_client.GetAccountInfoAsync();
            Console.WriteLine($"User profile: {accountInfo}");

            // Fetch all documents without any filters
            var allDocuments = await user_client.SearchDocumentsAsync();

            // Process the list of documents as needed
            foreach (var document in allDocuments.Where(x => x.Title == "FlowchartQuodsi"))
            {
                Console.WriteLine($"Document ID: {document.DocumentId}, Title: {document.Title}");

                string documentId = document.DocumentId;
                var documentContents = await user_client.GetDocumentContentsAsync(documentId);

                Console.WriteLine($"Document Title: {documentContents.Title}");
                foreach (var page in documentContents.Pages)
                {

                    Console.WriteLine($"Page Title: {page.Title}");

                    foreach (var dataPair in page.CustomData ?? Enumerable.Empty<DataPair>())
                    {
                        Console.WriteLine($"Page Data: {dataPair.Key} {dataPair.Value}");
                    }

                    foreach (var shape in page.Items.Shapes)
                    {
                        Console.WriteLine($"Shape ID: {shape.Id}, Class: {shape.Class}");
                        foreach (var item in shape.CustomData)
                        {
                            Console.WriteLine($"Shape Data: {item.Key} {item.Value}");
                        }

                    }
                    foreach (var line in page.Items.Lines)
                    {
                        Console.WriteLine($"Line ID: {line.Id}, Endpoint1: {line.Endpoint1}, Endpoint2: {line.Endpoint2}");
                        foreach (var item in line.CustomData)
                        {
                            Console.WriteLine($"Line Data: {item.Key} {item.Value}");
                        }

                    }
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during token introspection: {ex.Message}");
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