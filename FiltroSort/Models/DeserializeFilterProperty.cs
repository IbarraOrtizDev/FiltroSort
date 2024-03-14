namespace FilterSort.Models;
/// <summary>
///    Author:   Edwin Ibarra
///    Create Date: 14/03/2024
///    Es la clase que se encarga de deserializar las propiedades del filtro
/// </summary>
public class DeserializeFilterProperty
{
    public string? PropertyName { get; set; }
    public string? Value { get; set; }
    public List<string> Values { get; set; } = new List<string>();
    public string? Operator { get; set; }

}
