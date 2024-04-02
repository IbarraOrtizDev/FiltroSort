using System.Linq.Expressions;

namespace FilterSort.Helpers;

/// <summary>
///    Author:   Edwin Ibarra
///    Create Date: 14/03/2024
///    Class FilterCondition, esta clase se encarga de generar la expresion de condicion
/// </summary>
public class FilterCondition
{
    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 14/03/2024
    ///    BinaryExpression, este metodo se encarga de generar la expresion binaria, la cual se utiliza para generar la expresion de condicion, cuando el parametro de busqueda values no es vacio, se utiliza el operador IN o NOT IN
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="operatorFilter"></param>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    /// <param name="values"></param>
    /// <param name="typeValue"></param>
    /// <returns>
    /// Retorna la expresion binaria
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public static BinaryExpression BinaryExpression(string propertyName, string operatorFilter, ParameterExpression parameter, List<string> values, Type typeValue)
    {
        var property = Expression.Property(parameter, propertyName);

        if (values.Count > 1)
        {
            operatorFilter = operatorFilter == "!@=" ? "NOT IN" : "IN";
        }

        if (values[0].ToLower() == "null")
        {
            return operatorFilter switch
            {
                "==" => Expression.Equal(property, Expression.Constant(null)),
                _ => Expression.NotEqual(property, Expression.Constant(null)),
            };
        }

        Expression constant = GetExpressionConstant(operatorFilter, values, typeValue);

        if(Nullable.GetUnderlyingType(typeValue) != null && !operatorFilter.Contains("IN"))
            constant = Expression.Convert(constant, typeValue);

        if (constant == null) return null;

        if (typeValue.Name.Contains("List"))
        {
            return operatorFilter switch
            {
                "@=" => resolveGenericNegative(property, constant, "Equals", true),
                "!@=" => resolveGenericNegative(property, constant, "Equals", true),
                _ => resolveCountList(property, constant, operatorFilter)
            };
        }

        return operatorFilter switch
        {
            "==" => Expression.Equal(property, constant),
            "!=" => Expression.NotEqual(property, constant),
            ">" => Expression.GreaterThan(property, constant),
            "<" => Expression.LessThan(property, constant),
            ">=" => Expression.GreaterThanOrEqual(property, constant),
            "<=" => Expression.LessThanOrEqual(property, constant),
            "_=" => resolveGeneric(property, constant, "StartsWith"),
            "!_=" => resolveGenericNegative(property, constant, "StartsWith"),
            "_-=" => resolveGeneric(property, constant, "EndsWith"),
            "!_-=" => resolveGenericNegative(property, constant, "EndsWith"),
            "@=" => resolveGeneric(property, constant, "Contains"),
            "!@=" => resolveGenericNegative(property, constant, "Contains"),
            "@=*" => resolveGeneric(property, constant, "Contains", true),
            "_=*" => resolveGeneric(property, constant, "StartsWith", true),
            "_-=*" => resolveGeneric(property, constant, "EndsWith",true),
            "==*" => resolveEqualsIgnoreCase(property, constant),
            "!=*" => resolveGenericNegative(property, constant, "Equals", true),
            "!@=*" => resolveGenericNegative(property, constant, "Contains", true),
            "!_=*" => resolveGenericNegative(property, constant, "StartsWith", true),
            "IN" => resolveInOrNotIn(property, values, typeValue, true),
            "NOT IN" => resolveInOrNotIn(property, values, typeValue, false),
            _ => throw new ArgumentException("Invalid operator filter")
        };
    }

    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 14/03/2024
    ///    BinaryExpression, este metodo se encarga de generar la expresion binaria, es utilizado cuando el filtro es unico y no se establecio propiedad, en este caso se busca en todas las propiedades [Searchable] del modelo
    /// </summary>
    /// <param name="listProperties"></param>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    /// <returns>
    /// Retorna la expresion binaria
    /// </returns>
    public static BinaryExpression BinaryExpression<T>(List<string> listProperties, ParameterExpression parameter, List<string> values)
    {
        BinaryExpression binaryExpressionsReturn = null;
        Expression constant = GetExpressionConstant("@=", values, typeof(string));
        BinaryExpression expValidate = null;
        foreach (var property in listProperties)
        {
            if (typeof(T).GetProperty(property).PropertyType.Name.Contains("List"))
            {
                continue;
            }
            else
            {
                var propertyExp = Expression.Property(parameter, property);
                var toStringMethod = typeof(object).GetMethod("ToString");
                var toStringCall = Expression.Call(propertyExp, toStringMethod);
                MethodCallExpression callExpression = Expression.Call(toStringCall, "Contains", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
                expValidate = Expression.Equal(callExpression, Expression.Constant(true));
            }
            if (binaryExpressionsReturn == null)
                binaryExpressionsReturn = expValidate;
            else
                binaryExpressionsReturn = Expression.OrElse(binaryExpressionsReturn, expValidate);
        }
        return binaryExpressionsReturn;
    }

    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 14/03/2024
    ///    Description: Se encarga de generar el BinaryExpression, el metodo tipo de metodo a utilizar se recibe por parametro y de acuerdo al parametro ignoreCase se determina si lo ignora o no
    /// </summary>
    /// <param name="property"></param>
    /// <param name="constant"></param>
    /// <param name="method"></param>
    /// <param name="ignoreCase"></param>
    /// <returns>
    /// Retorna la expresion binaria
    /// </returns>
    private static BinaryExpression resolveGeneric(Expression propertyExp, Expression constant, string method, bool ignoreCase = false)
    {
        MethodCallExpression callExpression;
        if (!ignoreCase)
            callExpression = Expression.Call(propertyExp, method, null, constant);
        else
        {
            /*callExpression = Expression.Call(property, method, null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));*/
            var toStringMethod = typeof(object).GetMethod("ToString");
            var toStringCall = Expression.Call(propertyExp, toStringMethod);

            var toUpperMethod = typeof(string).GetMethod("ToUpper", Type.EmptyTypes);
            var toUpperCall = Expression.Call(toStringCall, toUpperMethod);

            var constantToUpper = Expression.Call(constant, toUpperMethod);
            if(method == "Equals")
            {
                var equalsMethod = typeof(string).GetMethod(method, new[] { typeof(string) });
                callExpression = Expression.Call(toUpperCall, equalsMethod, constantToUpper);
            }else
                callExpression = Expression.Call(toUpperCall, method, null, constantToUpper);
        }
        return Expression.Equal(callExpression, Expression.Constant(true));
    }

    private static BinaryExpression resolveCountList(Expression propertyExp, Expression constant, string method)
    {
        var countProperty = Expression.Property(propertyExp, "Count");
        var notNull = Expression.NotEqual(propertyExp, Expression.Constant(null));
        BinaryExpression binaryComparation = null;
        switch (method)
        {
            case ">":
                binaryComparation = Expression.GreaterThan(countProperty, constant);
                break;
            case "<":
                binaryComparation = Expression.LessThan(countProperty, constant);
                break;
            case ">=":
            binaryComparation = Expression.GreaterThanOrEqual(countProperty, constant);
                break;
            case "<=":
                binaryComparation = Expression.LessThanOrEqual(countProperty, constant);
                break;
            default:
                throw new ArgumentException("Invalid operator filter");
        };
        return Expression.AndAlso(notNull, binaryComparation);
    }
    private static BinaryExpression resolveEqualsIgnoreCase(Expression propertyExp, Expression constant)
    {
        MethodCallExpression callExpression;
        var toStringMethod = typeof(object).GetMethod("ToString");
        var toStringCall = Expression.Call(propertyExp, toStringMethod);

        var toUpperMethod = typeof(string).GetMethod("ToUpper", Type.EmptyTypes);
        var toUpperCall = Expression.Call(toStringCall, toUpperMethod);

        // Convert the constant to upper case
        var constantToUpper = Expression.Call(constant, toUpperMethod);

        return Expression.Equal(toUpperCall, constantToUpper);
    }

    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 14/03/2024
    ///    Description: Se encarga de generar el BinaryExpression, el metodo tipo de metodo a utilizar se recibe por parametro y de acuerdo al parametro ignoreCase se determina si lo ignora o no, en este caso siempre retorna negando el metodo
    /// </summary>
    /// <param name="property"></param>
    /// <param name="constant"></param>
    /// <param name="method"></param>
    /// <param name="ignoreCase"></param>
    /// <returns>
    /// Retorna la expresion binaria
    /// </returns>
    private static BinaryExpression resolveGenericNegative(Expression propertyExp, Expression constant, string method, bool ignoreCase = false)
    {
        MethodCallExpression callExpression;
        if (!ignoreCase)
            callExpression = Expression.Call(propertyExp, method, null, constant);
        else
        {
            var toStringMethod = typeof(object).GetMethod("ToString");
            var toStringCall = Expression.Call(propertyExp, toStringMethod);

            var toUpperMethod = typeof(string).GetMethod("ToUpper", Type.EmptyTypes);
            var toUpperCall = Expression.Call(toStringCall, toUpperMethod);

            var constantToUpper = Expression.Call(constant, toUpperMethod);
            if (method == "Equals")
            {
                var equalsMethod = typeof(string).GetMethod(method, new[] { typeof(string) });
                callExpression = Expression.Call(toUpperCall, equalsMethod, constantToUpper);
            }
            else
                callExpression = Expression.Call(toUpperCall, method, null, constantToUpper);
        }
        var negativeExpression = Expression.Not(callExpression);
        return Expression.Equal(negativeExpression, Expression.Constant(true));
    }


    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 14/03/2024
    ///    resolveInOrNotIn, este metodo se encarga de generar la expresion binaria donde evalua si el valor esta en la lista de valores, de acuerdo al parametro isIn, valida si el valor esta en la lista de valores o no
    /// </summary>
    /// <param name="property"></param>
    /// <param name="values"></param>
    /// <param name="typeValue"></param>
    /// <param name="isIn"></param>
    /// <returns></returns>
    private static BinaryExpression resolveInOrNotIn(Expression property, List<string> values, Type typeValue, bool isIn)
    {
        MethodCallExpression call = resolveContainsMethod(property, values, typeValue);

        if (isIn)
            return Expression.Equal(call, Expression.Constant(true));
        var notStartWith = Expression.Not(call);
        return Expression.NotEqual(notStartWith, Expression.Constant(true));
    }

    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 14/03/2024
    ///    resolveContainsMethod, este metodo se encarga de generar la expresion de llamada a metodo Contains, de acuerdo al tipo de valor
    /// </summary>
    /// <param name="property"></param>
    /// <param name="values"></param>
    /// <param name="typeValue"></param>
    /// <returns></returns>
    private static MethodCallExpression resolveContainsMethod(Expression property, List<string> values, Type typeValue)
    {
        MethodCallExpression call;
        typeValue = typeValueNotNull(typeValue);
        switch (typeValue)
        {
            case Type t when t == typeof(int):
                call = GenerateDinamicMethodCallExpression<int>.methodCall(property, values);
                break;
            case Type t when t == typeof(decimal):
                call = GenerateDinamicMethodCallExpression<decimal>.methodCall(property, values);
                break;
            case Type t when t == typeof(double):
                call = GenerateDinamicMethodCallExpression<double>.methodCall(property, values);
                break;
            case Type t when t == typeof(float):
                call = GenerateDinamicMethodCallExpression<float>.methodCall(property, values);
                break;
            case Type t when t == typeof(long):
                call = GenerateDinamicMethodCallExpression<long>.methodCall(property, values);
                break;
            case Type t when t == typeof(short):
                call = GenerateDinamicMethodCallExpression<short>.methodCall(property, values);
                break;
            case Type t when t == typeof(byte):
                call = GenerateDinamicMethodCallExpression<byte>.methodCall(property, values);
                break;
            case Type t when t == typeof(bool):
                call = GenerateDinamicMethodCallExpression<bool>.methodCall(property, values);
                break;
            case Type t when t == typeof(DateTime):
                call = GenerateDinamicMethodCallExpression<DateTime>.methodCall(property, values);
                break;
            default:
                call = GenerateDinamicMethodCallExpression<string>.methodCall(property, values);
                break;
        }
        return call;
    }

    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 14/03/2024
    ///    Get Expression Constant, este metodo se encarga de obtener la expresion constante
    /// </summary>
    /// <param name="operatorFilter"></param>
    /// <param name="value"></param>
    /// <param name="values"></param>
    /// <param name="typeValue"></param>
    /// <returns>
    /// Expression
    /// </returns>
    public static Expression GetExpressionConstant(string operatorFilter, List<string> values, Type typeValue)
    {
        Expression constant = null;
        try
        {
            typeValue = typeValueNotNull(typeValue);
            if (operatorFilter == "IN" || operatorFilter == "NOT IN")
            {
                switch (typeValue)
                {
                    case Type t when t == typeof(int):
                        List<int> intList = values.Select(x => Convert.ToInt32(x)).ToList();
                        constant = Expression.Constant(intList, typeof(List<int>));
                        break;
                    case Type t when t == typeof(decimal):
                        List<decimal> decimalList = values.Select(x => Convert.ToDecimal(x)).ToList();
                        constant = Expression.Constant(decimalList, typeof(List<decimal>));
                        break;
                    case Type t when t == typeof(double):
                        List<double> doubleList = values.Select(x => Convert.ToDouble(x)).ToList();
                        constant = Expression.Constant(doubleList, typeof(List<double>));
                        break;
                    case Type t when t == typeof(float):
                        List<float> floatList = values.Select(x => Convert.ToSingle(x)).ToList();
                        constant = Expression.Constant(floatList, typeof(List<float>));
                        break;
                    case Type t when t == typeof(long):
                        List<long> longList = values.Select(x => Convert.ToInt64(x)).ToList();
                        constant = Expression.Constant(longList, typeof(List<long>));
                        break;
                    case Type t when t == typeof(short):
                        List<short> shortList = values.Select(x => Convert.ToInt16(x)).ToList();
                        constant = Expression.Constant(shortList, typeof(List<short>));
                        break;
                    case Type t when t == typeof(byte):
                        List<byte> byteList = values.Select(x => Convert.ToByte(x)).ToList();
                        constant = Expression.Constant(byteList, typeof(List<byte>));
                        break;
                    case Type t when t == typeof(bool):
                        List<bool> boolList = values.Select(x => Convert.ToBoolean(x)).ToList();
                        constant = Expression.Constant(boolList, typeof(List<bool>));
                        break;
                    case Type t when t == typeof(DateTime):
                        List<DateTime> dateTimeList = values.Select(x => Convert.ToDateTime(x)).ToList();
                        constant = Expression.Constant(dateTimeList, typeof(List<DateTime>));
                        break;
                    default:
                        constant = Expression.Constant(values, typeof(List<string>));
                        break;
                }
            }
            else
            {
                if (typeValue == typeof(string))
                {
                    constant = Expression.Constant(values[0]);
                }else if (typeValue.Name.Contains("List"))
                {
                    constant = Expression.Constant(Int32.Parse(values[0]));
                }
                else
                {
                    constant = Expression.Constant(Convert.ChangeType(values[0], typeValue));
                }
            }
        }
        catch (Exception)
        {
            constant = null;
        }

        return constant;
    }

    public static Type typeValueNotNull(Type typeValue)
    {
           return Nullable.GetUnderlyingType(typeValue) ?? typeValue;
    }
}
