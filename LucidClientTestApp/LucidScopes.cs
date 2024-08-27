
public static class LucidScopes
{
    public static readonly List<string> UserScopes = new List<string>
        {
            "lucidchart.document.content",
            "lucidchart.document.content:readonly",
            "offline_access",
            "user.profile",
            "account.user:readonly",
            "account.info"
        };

    public static readonly List<string> AccountScopes = new List<string>
        {
            "offline_access",
            "account.user:readonly",
            "account.info"
        };
}

