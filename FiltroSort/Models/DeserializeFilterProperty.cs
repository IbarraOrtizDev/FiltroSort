using FilterSort.Helpers;

namespace FilterSort.Models;
/// <summary>
///    Author:   Edwin Ibarra
///    Create Date: 14/03/2024
///    Es la clase que se encarga de deserializar las propiedades del filtro
/// </summary>
public class DeserializeFilterProperty
{
    public string? PropertyName { get; set; }
    public List<string> Values { get; set; } = new List<string>();
    public string? Operator { get; set; }
    public DeserializeFilterProperty(string filterParamUnique, Type type, bool validOperator = true)
    {
        var listOperators = new List<string> { "!_=*", "!@=*", "_-=*", "!_-=", "_-=", "!@=", "!_=", "@=*", "_=*", "==*", "!=*", "==", "!=", ">=", "<=", "@=", "_=", ">", "<" };

        
        foreach (var item in listOperators)
        {
            if (filterParamUnique.Contains(item))
            {
                Operator = item;
                break;
            }
        }
        if (!string.IsNullOrEmpty(Operator))
        {
            if (validOperator && GenerateBinaryExpression<bool>.GetPropertyInfo(filterParamUnique.Split(Operator)[0], type) == null)
                return;
            PropertyName = filterParamUnique.Split(Operator)[0];
            var valor = filterParamUnique.Split(Operator)[1];
            if (!(valor == null || valor == string.Empty))
            {
                if (valor.Contains("|"))
                {
                    Values = valor.Split('|').ToList();
                }
                else
                {
                    Values.Add(valor);
                }

            }
        };
    }
}
