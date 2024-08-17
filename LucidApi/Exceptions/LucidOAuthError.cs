namespace LucidApi.Exceptions
{
    public class LucidOAuthException : Exception
    {
        public string ErrorCode { get; }
        public string Description { get; }

        public LucidOAuthException(string errorCode, string description)
            : base($"{errorCode}: {description}")
        {
            ErrorCode = errorCode;
            Description = description;
        }
    }
}