using FilterSort.Models;
using FiltroSort;
using System.Linq.Expressions;
using System.Reflection;

namespace FilterSort.Helpers;

public class GenerateBinaryExpression<T>
{
    /// <summary>
    /// Deserializa cada segmento de la cadena de filtro y para cada segmento genera la expresion binaria
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
            var conditionData = GetConditionExpression(filter);
            BinaryExpression condition = null;
            if (conditionData == null && listFilters.Count == 1)
            {
                condition = FilterCondition.BinaryExpression(propertiesList, parameter, filterParam);
            }
            else
            {
                if (conditionData == null || conditionData.PropertyName == null || !OperatorIsValidForType(conditionData.Operator, typeof(T).GetProperty(conditionData.PropertyName).PropertyType)) continue;

                if ((conditionData.Value == null && conditionData.Values == null)
                    || !propertiesList.Contains(conditionData.PropertyName)) continue;
                var property = typeof(T).GetProperty(conditionData.PropertyName);
                condition = FilterCondition.BinaryExpression(conditionData.PropertyName, conditionData.Operator, parameter, conditionData.Value, conditionData.Values, property.PropertyType);
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
    /// Deserializa cada segmento de la cadena de filtro
    /// </summary>
    /// <param name="filterParamUnique"></param>
    /// <returns>
    /// Retorna un objeto de tipo DeserializeFilterProperty, donde esta la propiedad, el valor y el operador
    /// </returns>
    private static DeserializeFilterProperty GetConditionExpression(string filterParamUnique)
    {
        var listOperators = new List<string> { "!_=*", "!@=*", "_-=*", "!_-=", "_-=", "!@=", "!_=", "@=*", "_=*", "==*", "!=*", "==", "!=", ">=", "<=", "@=", "_=", ">", "<" };

        string operatorFilter = string.Empty;

        foreach (var item in listOperators)
        {
            if (filterParamUnique.Contains(item))
            {
                operatorFilter = item;
                break;
            }
        }
        if (operatorFilter == null || operatorFilter == string.Empty) return null;
        DeserializeFilterProperty deserializeFilterProperty = new DeserializeFilterProperty();
        deserializeFilterProperty.Operator = operatorFilter;
        if (typeof(T).GetProperty(filterParamUnique.Split(operatorFilter)[0]) == null) return deserializeFilterProperty;
        deserializeFilterProperty.PropertyName = filterParamUnique.Split(operatorFilter)[0];

        var valor = filterParamUnique.Split(operatorFilter)[1];
        if (!(valor == null || valor == string.Empty))
        {
            if (valor.Contains("|"))
            {
                deserializeFilterProperty.Values = valor.Split('|').ToList();
            }
            else
            {
                deserializeFilterProperty.Value = valor;
            }

        }
        return deserializeFilterProperty;
    }
    /// <summary>
    /// Operator is valid for type, evaluamos si el operador es valido para el tipo de dato que se esta evaluando
    /// </summary>
    /// <param name="operatorFilter"></param>
    /// <param name="type"></param>
    /// <returns>
    /// Retorna un valor booleano
    /// </returns>
    private static bool OperatorIsValidForType(string operatorFilter, Type type)
    {
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
        }
        return false;
    }

    /// <summary>
    /// Lista las propiedades que son buscables
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
