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
        Expression constant = GetExpressionConstant(operatorFilter, values, typeValue);

        if (constant == null) return null;

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
    public static BinaryExpression BinaryExpression(List<string> listProperties, ParameterExpression parameter, List<string> values)
    {
        BinaryExpression binaryExpressionsReturn = null;
        Expression constant = GetExpressionConstant("@=", values, typeof(string));
        foreach (var property in listProperties)
        {
            var propertyExp = Expression.Property(parameter, property);
            var toStringMethod = typeof(object).GetMethod("ToString");
            var toStringCall = Expression.Call(propertyExp, toStringMethod);
            MethodCallExpression callExpression = Expression.Call(toStringCall, "Contains", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            var expValidate = Expression.Equal(callExpression, Expression.Constant(true));
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
            if (operatorFilter == "IN" || operatorFilter == "NOT IN")
            {
                if (typeValue == typeof(int))
                {
                    List<int> list = values.Select(x => Convert.ToInt32(x)).ToList();
                    constant = Expression.Constant(list, typeof(List<int>));
                }
                else if (typeValue == typeof(decimal))
                {
                    List<decimal> list = values.Select(x => Convert.ToDecimal(x)).ToList();
                    constant = Expression.Constant(list, typeof(List<decimal>));
                }
                else if (typeValue == typeof(double))
                {
                    List<double> list = values.Select(x => Convert.ToDouble(x)).ToList();
                    constant = Expression.Constant(list, typeof(List<double>));
                }
                else if (typeValue == typeof(float))
                {
                    List<float> list = values.Select(x => Convert.ToSingle(x)).ToList();
                    constant = Expression.Constant(list, typeof(List<float>));
                }
                else if (typeValue == typeof(long))
                {
                    List<long> list = values.Select(x => Convert.ToInt64(x)).ToList();
                    constant = Expression.Constant(list, typeof(List<long>));
                }
                else if (typeValue == typeof(short))
                {
                    List<short> list = values.Select(x => Convert.ToInt16(x)).ToList();
                    constant = Expression.Constant(list, typeof(List<short>));
                }
                else if (typeValue == typeof(byte))
                {
                    List<byte> list = values.Select(x => Convert.ToByte(x)).ToList();
                    constant = Expression.Constant(list, typeof(List<byte>));
                }
                else if (typeValue == typeof(bool))
                {
                    List<bool> list = values.Select(x => Convert.ToBoolean(x)).ToList();
                    constant = Expression.Constant(list, typeof(List<bool>));
                }
                else if (typeValue == typeof(DateTime))
                {
                    List<DateTime> list = values.Select(x => Convert.ToDateTime(x)).ToList();
                    constant = Expression.Constant(list, typeof(List<DateTime>));
                }
                else
                {
                    constant = Expression.Constant(values, typeof(List<string>));
                }
            }
            else
            {
                if (typeValue == typeof(string))
                {
                    constant = Expression.Constant(values[0]);
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
}
