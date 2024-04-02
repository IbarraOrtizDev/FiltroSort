using FilterSort.Models;
using FiltroSort;
using System.Linq.Expressions;
using System.Reflection;

namespace FilterSort.Helpers;

/// <summary>
///    Author:   Edwin Ibarra
///    Create Date: 14/03/2024
///    Description: Clase que se encarga de generar el BinaryExpression principal para luego convertirlo en una Lambda
/// </summary>
/// <typeparam name="T"></typeparam>
public class GenerateBinaryExpression<T>
{
    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 14/03/2024
    ///    Deserializa cada segmento de la cadena de filtro y para cada segmento genera la expresion binaria
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="filterParam"></param>
    /// <returns>
    /// Retorna una expresion binaria, de acuerdo a los segmentos de la cadena de filtro
    /// </returns>
    public static BinaryExpression GetFilterExpressionGenerator(ParameterExpression parameter, string filterParam)
    {
        if (string.IsNullOrWhiteSpace(filterParam)) return null;

        var propertiesList = GetSearchableProperties<T>();

        var listFilters = filterParam.Split(',').ToList();
        BinaryExpression binaryExpressions = null;

        foreach (var filter in listFilters)
        {
            var conditionData = new DeserializeFilterProperty<T>(filter);
            BinaryExpression condition = null;
            if (string.IsNullOrEmpty(conditionData.PropertyName) && string.IsNullOrEmpty(conditionData.Operator))
            {
                condition = FilterCondition.BinaryExpression<T>(propertiesList, parameter, new List<string>() { filter });
            }
            else
            {
                if (!(conditionData.Values.FirstOrDefault()?.ToLower() == "null" && conditionData.PropertyName != null)
                    && (conditionData.PropertyName == null
                    || (conditionData.Values == null && conditionData.Values.Count == 0)
                    || !OperatorIsValidForType(conditionData.Operator, typeof(T).GetProperty(conditionData.PropertyName).PropertyType)
                    || !propertiesList.Contains(conditionData.PropertyName))
                    ) continue;

                var property = typeof(T).GetProperty(conditionData.PropertyName);
                condition = FilterCondition.BinaryExpression(conditionData.PropertyName, conditionData.Operator, parameter, conditionData.Values, property.PropertyType);
                if (condition == null) continue;
            };

            if (binaryExpressions == null)
                binaryExpressions = condition;
            else
                binaryExpressions = Expression.AndAlso(binaryExpressions, condition);
        }

        return binaryExpressions;
    }
    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 14/03/2024
    ///    Operator is valid for type, evaluamos si el operador es valido para el tipo de dato que se esta evaluando
    /// </summary>
    /// <param name="operatorFilter"></param>
    /// <param name="type"></param>
    /// <returns>
    /// Retorna un valor booleano
    /// </returns>
    private static bool OperatorIsValidForType(string operatorFilter, Type type)
    {
        type = FilterCondition.typeValueNotNull(type);
        if (type == typeof(string))
        {
            return operatorFilter switch
            {
                "==" => true,
                "!=" => true,
                "@=" => true,
                "_=" => true,
                "_-=" => true,
                "!@=" => true,
                "!_=" => true,
                "!_-=" => true,
                "@=*" => true,
                "_=*" => true,
                "_-=*" => true,
                "==*" => true,
                "!=*" => true,
                "!@=*" => true,
                "!_=*" => true,
                _ => false
            };
        }
        else if (type == typeof(int) || type == typeof(double) || type == typeof(decimal))
        {
            return operatorFilter switch
            {
                "==" => true,
                "!=" => true,
                ">" => true,
                "<" => true,
                ">=" => true,
                "<=" => true,
                _ => false
            };
        }
        else if (type == typeof(DateTime))
        {
            return operatorFilter switch
            {
                "==" => true,
                "!=" => true,
                ">" => true,
                "<" => true,
                ">=" => true,
                "<=" => true,
                _ => false
            };
        }
        else if (type == typeof(bool))
        {
            return operatorFilter switch
            {
                "==" => true,
                "!=" => true,
                _ => false
            };
        }else if (type.Name.Contains("List"))
        {
            return operatorFilter switch
            {
                ">" => true,
                "<" => true,
                ">=" => true,
                "<=" => true,
                _ => false
            };
        }
        return false;
    }

    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 14/03/2024
    ///    Lista las propiedades que son buscables
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>
    /// Lista de propiedades que son buscables
    /// </returns>
    public static List<string> GetSearchableProperties<T>()
    {
        return typeof(T).GetProperties()
            .Where(e => e.GetCustomAttribute<Searchable>(true) != null).Select(x => x.Name).ToList();
    }
}
