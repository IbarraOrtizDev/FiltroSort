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

        var propertiesList = GetSearchableProperties(typeof(T));

        var listFilters = filterParam.Split(',').ToList();
        BinaryExpression binaryExpressions = null;

        foreach (var filter in listFilters)
        {
            var conditionData = new DeserializeFilterProperty(filter, typeof(T));
            BinaryExpression condition = null;
            if (string.IsNullOrEmpty(conditionData.PropertyName) && string.IsNullOrEmpty(conditionData.Operator))
            {
                condition = FilterCondition.BinaryExpression<T>(propertiesList, parameter, filter);
            }
            else
            {
                if (!PropertyAndValueIsAvailable(filter, typeof(T))) continue;

                var property = GetPropertyInfo(conditionData.PropertyName!, typeof(T));
                if (property == null) continue;

                condition = BinaryExpressionByProperty(conditionData.PropertyName, conditionData.Operator, parameter, conditionData.Values, property.PropertyType, typeof(T));
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
    ///     Author:   Edwin Ibarra
    ///     Create Date: 05/04/2024
    ///     En un paso anterior cuando se recibia un filtro con una propiedad que incluyera un elemento "|", este generaba una expresion lambda para evaluar si el valor de la propiedad estaba contenido en la lista de valores recibida, ahora se ha modificado para que se pueda evaluar cada valor de la lista de valores recibida con el valor de la propiedad y por cada valor se genera una expresion lambda, para luego unirlas con un OR y de esta manera poder utilizar el operador correspondiente al filtro
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="operatorFilter"></param>
    /// <param name="parameter"></param>
    /// <param name="values"></param>
    /// <param name="typeValue"></param>
    /// <param name="typeValuePrincipal"></param>
    /// <returns></returns>
    private static BinaryExpression BinaryExpressionByProperty(string propertyName, string operatorFilter, ParameterExpression parameter, List<string> values, Type typeValue, Type typeValuePrincipal)
    {
        BinaryExpression binaryExpressions = null;

        foreach (var value in values)
        {
            var evaluation = FilterCondition.BinaryExpression(propertyName, operatorFilter, parameter, value, typeValue, typeValuePrincipal);
            if(binaryExpressions == null)
                binaryExpressions = evaluation;
            else
                binaryExpressions = Expression.OrElse(binaryExpressions, evaluation);
        }
        return binaryExpressions;
    }
    /// <summary>
    ///     Author:   Edwin Ibarra
    ///     Create Date: 03/04/2024
    ///     Evalua si la propiedad y el valor estan disponibles y corresponden al tipo de dato establecido
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="typeValue"></param>
    /// <returns></returns>
    private static bool PropertyAndValueIsAvailable(string filter, Type typeValue)
    {
        var propertiesList = GetSearchableProperties(typeValue);
        if (filter.Contains("."))
        {
            bool found = false;
            filter.Split('.').ToList().ForEach(x =>
            {
                if (propertiesList.Contains(x))
                {
                    propertiesList = GetSearchableProperties(typeValue.GetProperty(x).PropertyType);
                    if(typeValue.GetProperty(x).PropertyType.Name.Contains("List") && !FilterCondition.IsPrimitiveExtenssionProperty(typeValue.GetProperty(x).PropertyType.GetGenericArguments()[0]))
                    {
                        typeValue = typeValue.GetProperty(x).PropertyType.GetGenericArguments()[0];
                    }
                    else
                        typeValue = typeValue.GetProperty(x).PropertyType;
                    filter = filter.Replace(x + ".", "");
                    found = true;
                }
            });
            if (!found) return false;
        }
        var conditionData = new DeserializeFilterProperty(filter, typeValue);

        if (!(conditionData.Values.FirstOrDefault()?.ToLower() == "null" && conditionData.PropertyName != null)
                    && (conditionData.PropertyName == null
                    || (conditionData.Values == null && conditionData.Values.Count == 0)
                    || !OperatorIsValidForType(conditionData.Operator, typeValue.GetProperty(conditionData.PropertyName).PropertyType)
                    || !propertiesList.Contains(conditionData.PropertyName))
                    ) return false;
        return true;
    }

    /// <summary>
    ///     Author:   Edwin Ibarra
    ///     Create Date: 03/04/2024
    ///     Obtiene la informacion de la ultima propiedad de la cadena de propiedades, de acuerdo al filtro dado por el usuario
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="typeValue"></param>
    /// <returns></returns>
    public static PropertyInfo? GetPropertyInfo(string propertyName, Type typeValue)
    {
        if (propertyName.Contains("."))
        {
            var properties = propertyName.Split('.');
            var property = typeValue.GetProperty(properties[0]);
            if (property == null) return null;
            if(property.PropertyType.Name.Contains("List") && FilterCondition.IsPrimitiveExtenssionProperty(property.PropertyType))
            {
                return GetPropertyInfo(propertyName.Replace(properties[0] + ".", ""), property.PropertyType.GetGenericArguments()[0]);
            }
            return GetPropertyInfo(propertyName.Replace(properties[0] + ".", ""), property.PropertyType);
        }
        PropertyInfo? propertyInfo = typeValue.GetProperty(propertyName);
        return propertyInfo;
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
                "@=" => true,
                "!@=" => true,
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
    /// <param name="typeObject"></param>
    /// <returns>
    /// Lista de propiedades que son buscables
    /// </returns>
    public static List<string> GetSearchableProperties(Type typeObject)
    {
        if(typeObject.Name.Contains("List") && !FilterCondition.IsPrimitiveExtenssionProperty(typeObject.GenericTypeArguments[0]))
        {
            return GetSearchableProperties(typeObject.GetGenericArguments()[0]);
        }
        return typeObject.GetProperties()
            .Where(e => e.GetCustomAttribute<Searchable>(true) != null).Select(x => x.Name).ToList();
    }
}
