namespace LucidApi.Models
{
    public class Shape
    {
        public string Id { get; set; }
        public string Class { get; set; }
        public List<TextArea> TextAreas { get; set; }
        public List<DataPair> CustomData { get; set; }
        public List<LinkedData> LinkedData { get; set; }
    }
}