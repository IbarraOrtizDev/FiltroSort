namespace FilterSort.Models;

public class DeserializeFilterProperty
{
    public string? PropertyName { get; set; }
    public string? Value { get; set; }
    public List<string> Values { get; set; } = new List<string>();
    public string? Operator { get; set; }

}
