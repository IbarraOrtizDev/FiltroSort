namespace FilterSort.Models
{
    public class DeserializeFilterProperty
    {
        public string PropertyName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public List<string> Values { get; set; } = new List<string>();
        public string Operator { get; set; } = string.Empty;
    }
}
