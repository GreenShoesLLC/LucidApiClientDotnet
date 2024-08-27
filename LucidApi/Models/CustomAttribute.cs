namespace LucidApi.Models
{
    public class CustomAttribute
    {
        public AttributeType Type { get; set; } = AttributeType.Number;
        public string? Name { get; set; }
        public object? Value { get; set; }
    }
}