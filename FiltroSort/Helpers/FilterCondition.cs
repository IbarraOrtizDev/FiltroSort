using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace FilterSort.Helpers;

/// <summary>
///    Author:   Edwin Ibarra
///    Create Date: 14/03/2024
///    Class FilterCondition, esta clase se encarga de generar la expresion de condicion
/// </summary>
public class FilterCondition
{

    /// <summary>
    /// Recibe la expresion de condicion y la convierte en una expresion lambda, la cual se utiliza para filtrar los datos
    /// </summary>
    /// <param name="typeValuePrincipal"></param>
    /// <param name="propertyNamePath"></param>
    /// <param name="propertyNamePathEvaluado"></param>
    /// <param name="operatorFilter"></param>
    /// <param name="value"></param>
    /// <param name="parameter"></param>
    /// <param name="property"></param>
    /// <param name="acc"></param>
    /// <returns></returns>
    public static BinaryExpression BinaryExpression(Type typeValuePrincipal, string propertyNamePath, string? propertyNamePathEvaluado, string operatorFilter, string value, ParameterExpression parameter,  MemberExpression? property, BinaryExpression? acc)
    {
        if (!propertyNamePath.Contains("."))
        {
            var respB = BinaryExpressionFinally(propertyNamePath, operatorFilter, parameter, value, typeValuePrincipal.Name.StartsWith("List") ? typeValuePrincipal.GetGenericArguments()[0].GetProperty(propertyNamePath).PropertyType : typeValuePrincipal.GetProperty(propertyNamePath).PropertyType, typeValuePrincipal, property);
            if (acc == null)
                return respB;
            return Expression.AndAlso(acc, respB);
        }
        var properties = propertyNamePath.Split(".");

        string propertyEvaluate = properties[0]; // propertyNamePathEvaluado == null ? properties[0] : (propertyNamePathEvaluado + "." + properties[0]);
        foreach (string element in propertyEvaluate.Split("."))
        {
            if (property == null)
                property = Expression.Property(parameter, element);
            else
                property = Expression.Property(property, element);
        }

        var propertyNotNull = Expression.NotEqual(property, Expression.Constant(null));
        var propertyType = typeValuePrincipal.GetProperty(properties[0]).PropertyType;
        acc = acc == null ? propertyNotNull : Expression.AndAlso(acc, propertyNotNull);
        //Evaluar si la propiedad es una lista
        if (typeValuePrincipal.GetProperty(properties[0]).PropertyType.Name.Contains("List"))
        {
            var respAny = BinaryExpressionAnyInList(typeValuePrincipal, propertyNamePath, operatorFilter, value, property);
            if (acc == null)
                return respAny;
            return Expression.AndAlso(acc, respAny);
        }else
            return BinaryExpression(propertyType, propertyNamePath.Replace(properties[0] + ".", ""), propertyEvaluate, operatorFilter, value, parameter, property, acc);
    }

    /// <summary>
    /// Se crea para evaluar propiedades de tipo lista de objetos cuando se debe aplicar any a la consulta
    /// </summary>
    /// <param name="typeValuePrincipal"></param>
    /// <param name="propertyNamePath"></param>
    /// <param name="operatorFilter"></param>
    /// <param name="value"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    public static BinaryExpression BinaryExpressionAnyInList(Type typeValuePrincipal, string propertyNamePath, string operatorFilter, string value, MemberExpression? property)
    {
        var propertyNameLeft = propertyNamePath.Split(".")[0];
        propertyNamePath = propertyNamePath.Replace(propertyNameLeft + ".", "");
        var letter = (property == null || property.Expression.ToString() == "x") ? "a" : nextLetter(property.Expression.ToString().Substring(0,1));

        var parameterY = Expression.Parameter(typeValuePrincipal.GetProperty(propertyNameLeft).PropertyType.GenericTypeArguments[0], letter);

        var resp = BinaryExpression(typeValuePrincipal.GetProperty(propertyNameLeft).PropertyType.GenericTypeArguments[0], propertyNamePath, null, operatorFilter, value, parameterY, null, null);

        var lambda = Expression.Lambda(resp, parameterY);

        var anyCall = Expression.Call(
            typeof(Enumerable),
            "Any",
            new Type[] { typeValuePrincipal.GetProperty(propertyNameLeft).PropertyType.GenericTypeArguments[0] },
            property,
            lambda
        );

        return Expression.Equal(anyCall, Expression.Constant(true));
    }

    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 29/11/2024
    ///     Metodo para buscar la siguiente letra en orden del abecedario
    /// </summary>
    /// <param name="letter"></param>
    /// <returns></returns>
    public static string nextLetter(string letter)
    {
        int position = char.Parse(letter) - 'a';
        var next = (char)('a' + (position + 1 % 26));
        return next.ToString();
    }

    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 14/03/2024
    ///    BinaryExpressionFinally, este metodo se encarga de generar la expresion binaria, la cual se utiliza para generar la expresion de condicion, cuando el parametro de busqueda values no es vacio, se utiliza el operador IN o NOT IN
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="operatorFilter"></param>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    /// <param name="typeValue"></param>
    /// <param name="typeValuePrincipal"></param>
    /// <param name="property"></param>
    /// <returns>
    /// Retorna la expresion binaria
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public static BinaryExpression BinaryExpressionFinally(string propertyName, string operatorFilter, ParameterExpression parameter, string value, Type typeValue, Type typeValuePrincipal, MemberExpression property = null)
    {
        if(!(propertyName.Contains(".") && typeValuePrincipal.GetProperty(propertyName.Split(".")[0]).PropertyType.Name.Contains("List")) )
        {
            foreach (string element in propertyName.Split("."))
            {
                if (property == null)
                    property = Expression.Property(parameter, element);
                else
                    property = Expression.Property(property, element);
            }
        }

        // If the value is null
        if (value.ToLower() == "null")
        {
            return operatorFilter switch
            {
                "==" => generateBinaryExpressionIgnoreNullInSubObject(propertyName, parameter, Expression.Equal(property, Expression.Constant(null))),
                _ => generateBinaryExpressionIgnoreNullInSubObject(propertyName, parameter, Expression.NotEqual(property, Expression.Constant(null))),
            };
        }

        if (propertyName.Contains(".") && typeValuePrincipal.GetProperty(propertyName.Split(".")[0]).PropertyType.Name.Contains("List"))
        {
            var propiedadListaObjetos = Expression.Property(parameter, propertyName.Split(".")[0]);
            var removeElementParent = propertyName.Replace(propertyName.Split(".")[0] + ".", "");

            LambdaExpression lambda = CreateAnyExpression(typeValuePrincipal.GetProperty(propertyName.Split(".")[0]).PropertyType.GenericTypeArguments[0], removeElementParent, operatorFilter, value);
            var anyMethod = Expression.Call(typeof(Enumerable), "Any", new[] { typeValuePrincipal.GetProperty(propertyName.Split(".")[0]).PropertyType.GetGenericArguments()[0] }, propiedadListaObjetos, lambda);
            return generateBinaryExpressionIgnoreNullInSubObject(propertyName, parameter, Expression.Equal(anyMethod, Expression.Constant(true)));
        }

        Expression constant = GetExpressionConstant(operatorFilter, value, typeValue);

        if (Nullable.GetUnderlyingType(typeValue) != null)
            constant = Expression.Convert(constant, typeValue);

        if (constant == null) return null;

        //Evalua si la propiedad es una lista, si es asi, se evalua si el valor esta en la lista o no o si la cantidad de elementos es mayor o menor a un valor
        if (typeValue.Name.Contains("List"))
        {
            return operatorFilter switch
            {
                "@=" => generateBinaryExpressionIgnoreNullInSubObject(propertyName, parameter, resolveContainsList(property, constant, typeValue, "@=")),
                "!@=" => generateBinaryExpressionIgnoreNullInSubObject(propertyName, parameter, resolveContainsList(property, constant, typeValue, "!@=")),
                _ => generateBinaryExpressionIgnoreNullInSubObject(propertyName, parameter, resolveCountList(property, constant, operatorFilter))
            };
        }
        if(typeValue == typeof(string)  && (operatorFilter.StartsWith("<") || operatorFilter.StartsWith(">")))
            return generateBinaryExpressionIgnoreNullInSubObject(propertyName, parameter, EvaluateLengthText(property, constant, operatorFilter));

        return generateBinaryExpressionIgnoreNullInSubObject(propertyName, parameter, EvaluateTypePrimitive(property, constant, operatorFilter));
    }

    /// <summary>
    /// Author:   Edwin Ibarra
    /// Create Date: 04/04/2024
    /// Crea la funcion lambda para evaluar si el valor esta en la lista de valores, es utilizado cuando la propiedad es una lista
    /// </summary>
    /// <param name="typeProperty"></param>
    /// <param name="propertyName"></param>
    /// <param name="operatorFilter"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private static LambdaExpression CreateAnyExpression(Type typeProperty, string propertyName, string operatorFilter, string value )
    {
        string propertyNameLeft = propertyName.Split(".")[0];
        var parameterY = Expression.Parameter(typeProperty, "y");
        MemberExpression propertyY = Expression.Property(parameterY, propertyNameLeft);

        var subQuery= BinaryExpressionFinally(propertyName, operatorFilter, parameterY, value, typeProperty.GetProperty(propertyNameLeft).PropertyType, typeProperty, propertyY);
        var lambda = Expression.Lambda(subQuery, parameterY);
        return lambda;
    }

    /// <summary>
    /// Author:   Edwin Ibarra
    /// Create Date: 04/04/2024
    /// Evalua condiciones cuando la propiedad es primitiva
    /// </summary>
    /// <param name="property"></param>
    /// <param name="constant"></param>
    /// <param name="operatorFilter"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static BinaryExpression EvaluateTypePrimitive(Expression property, Expression constant, string operatorFilter)
    {
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
            "_-=*" => resolveGeneric(property, constant, "EndsWith", true),
            "==*" => resolveEqualsIgnoreCase(property, constant),
            "!=*" => resolveGenericNegative(property, constant, "Equals", true),
            "!@=*" => resolveGenericNegative(property, constant, "Contains", true),
            "!_=*" => resolveGenericNegative(property, constant, "StartsWith", true),
            _ => throw new ArgumentException("Invalid operator filter")
        };
    }

    private static BinaryExpression EvaluateLengthText(Expression property, Expression constant, string operatorFilter)
    {
        return operatorFilter switch
        {
            ">" => Expression.GreaterThan(Expression.Property(property, "Length"), constant),
            "<" => Expression.LessThan(Expression.Property(property, "Length"), constant),
            ">=" => Expression.GreaterThanOrEqual(Expression.Property(property, "Length"), constant),
            "<=" => Expression.LessThanOrEqual(Expression.Property(property, "Length"), constant),
            _ => throw new ArgumentException("Invalid operator filter")
        };
    }

    /// <summary>
    ///     Author:   Edwin Ibarra
    ///     Create Date: 03/04/2024
    ///     Se encarga de generar la expresion binaria para validar si antes de evaluar una propiedad, las propiedades que la preceden son diferentes de null, solo aplica para cuando estas accediendo a un subobjeto
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="parameter"></param>
    /// <param name="binaryExpressionPrincipal"></param>
    /// <returns></returns>
    private static BinaryExpression generateBinaryExpressionIgnoreNullInSubObject(string propertyName, ParameterExpression parameter, BinaryExpression binaryExpressionPrincipal)
    {
        
        if (propertyName.Contains("."))
        {
            BinaryExpression binaryExpressionsReturn = null;
            Expression lastExpression = null;
            foreach (var propertyobj in propertyName.Split("."))
            {
                lastExpression = Expression.Property(lastExpression == null ? parameter : lastExpression, propertyobj);

                if(binaryExpressionsReturn == null)
                    binaryExpressionsReturn = Expression.NotEqual(lastExpression, Expression.Constant(null));
                else
                    binaryExpressionsReturn = Expression.AndAlso(binaryExpressionsReturn, Expression.NotEqual(lastExpression, Expression.Constant(null)));
                return Expression.AndAlso(binaryExpressionsReturn, binaryExpressionPrincipal);
            }
        }
        return binaryExpressionPrincipal;
    }


    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 14/03/2024
    ///    BinaryExpression, este metodo se encarga de generar la expresion binaria, es utilizado cuando el filtro es unico y no se establecio propiedad, en este caso se busca en todas las propiedades [Searchable] del modelo, pero solo las de primer nivel y tambien excluye las listas
    /// </summary>
    /// <param name="listProperties"></param>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    /// <returns>
    /// Retorna la expresion binaria
    /// </returns>
    public static BinaryExpression BinaryExpression<T>(List<string> listProperties, ParameterExpression parameter, string value)
    {
        BinaryExpression binaryExpressionsReturn = null;
        Expression constant = GetExpressionConstant("@=", value, typeof(string));
        foreach (var property in listProperties)
        {
            Type typeProperty = typeof(T).GetProperty(property).PropertyType;

            if (typeof(T).GetProperty(property).PropertyType.Name.Contains("List") || !IsPrimitiveExtenssionProperty(typeProperty))
            {
                continue;
            }
            var propertyExp = Expression.Property(parameter, property);
            MethodCallExpression toStringCall;
            if (FilterCondition.typeValueNotNull(typeProperty) == typeof(DateTime))
            {
                var hasValueProperty = Expression.Property(propertyExp, "HasValue");
                var valueProperty = Expression.Property(propertyExp, "Value");
                var toStringMethod = typeof(DateTime).GetMethod("ToString", new[] { typeof(string) });
                toStringCall = Expression.Call(valueProperty, toStringMethod, Expression.Constant("MM-dd-yyyy hh:mm tt"));
            }
            else
            {
                var toStringMethod = typeof(object).GetMethod("ToString");
                toStringCall = Expression.Call(propertyExp, toStringMethod);
            }
            var callExpression = Expression.Call(toStringCall, "Contains", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));

            BinaryExpression expValidate = Expression.Equal(callExpression, Expression.Constant(true));



            if (binaryExpressionsReturn == null)
                binaryExpressionsReturn = expValidate;
            else
                binaryExpressionsReturn = Expression.OrElse(binaryExpressionsReturn, expValidate);
        }
        return binaryExpressionsReturn;
    }

    /// <summary>
    /// Autor:  Edwin Ibarra
    /// Create Date: 04/04/2024
    /// Evalua si el tipo de dato es propio del sistema 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsPrimitiveExtenssionProperty(Type type)
    {
        return type.IsPrimitive || type.IsValueType || type == typeof(string) || type.Namespace.StartsWith("System");
    }


    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 14/03/2024
    ///    Description: Se encarga de generar el BinaryExpression, el metodo tipo de metodo a utilizar se recibe por parametro y de acuerdo al parametro ignoreCase se determina si lo ignora o no
    /// </summary>
    /// <param name="propertyExp"></param>
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
            var continueMethod = propertyExp.Type.Name == "String" ? propertyExp : toStringCall;

            var toUpperMethod = typeof(string).GetMethod("ToUpper", Type.EmptyTypes);
            var toUpperCall = Expression.Call(continueMethod, toUpperMethod);

            var constantToUpper = Expression.Call(constant, toUpperMethod);
            if(method == "Equals")
            {
                //var equalsMethod = typeof(string).GetMethod(method, new[] { typeof(string) });
                callExpression = constantToUpper;
            }else
                callExpression = Expression.Call(toUpperCall, method, null, constantToUpper);
        }
        return Expression.Equal(callExpression, Expression.Constant(true));
    }

    /// <summary>
    ///     Author:   Edwin Ibarra
    ///     Create Date: 02/04/2024
    ///     Se encarga de generar la expresion binaria para evaluar si el valor esta en la lista de valores
    /// </summary>
    /// <param name="propertyExp"></param>
    /// <param name="constant"></param>
    /// <param name="typeValue"></param>
    /// <param name="method"></param>
    /// <returns>BinaryExpression</returns>
    private static BinaryExpression resolveContainsList(Expression propertyExp, Expression constant,Type typeValue, string method)
    {
        var notNull = Expression.NotEqual(propertyExp, Expression.Constant(null));
        var contains = Expression.Call(propertyExp, typeValue.GetMethod("Contains", new[] { GetTypeList(typeValue) }), constant);
        if(method == "!@=")
        {
            return Expression.AndAlso(notNull,Expression.Equal(Expression.Not(contains), Expression.Constant(true)));
        }
        var evaluationContain = Expression.Equal(contains, Expression.Constant(true));
        return Expression.AndAlso(notNull,evaluationContain);

    }

    /// <summary>
    ///     Author:   Edwin Ibarra
    ///     Create Date: 02/04/2024
    ///     Retorna la expresion binaria para evaluar la cantidad de elementos en una lista
    /// </summary>
    /// <param name="propertyExp"></param>
    /// <param name="constant"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
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
            case "==":
                binaryComparation = Expression.Equal(countProperty, constant);
                break;
            default:
                throw new ArgumentException("Invalid operator filter");
        };
        return Expression.AndAlso(notNull, binaryComparation);
    }

    /// <summary>
    ///     Author:   Edwin Ibarra
    ///     Create Date: 03/04/2024
    ///     Este metodo evalua si una propiedad es igual a otra e ignora los case sensitive
    /// </summary>
    /// <param name="propertyExp"></param>
    /// <param name="constant"></param>
    /// <returns></returns>
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
    /// <param name="propertyExp"></param>
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
    ///    resolveContainsMethod, este metodo se encarga de generar la expresion de llamada a metodo Contains, de acuerdo al tipo de valor, este se ejecuta cuando el valor es una lista de valores enviada por el usuario
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
    /// <param name="typeValue"></param>
    /// <returns>
    /// Expression
    /// </returns>
    public static Expression GetExpressionConstant(string operatorFilter, string value, Type typeValue)
    {
        Expression constant = null;
        try
        {
            typeValue = typeValueNotNull(typeValue);
            if (typeValue == typeof(string))
            {
                if((!operatorFilter.StartsWith("<") && !operatorFilter.StartsWith(">")))
                    constant = Expression.Constant(value);
                else
                    constant = Expression.Constant(Convert.ChangeType(value.Replace(".", ","), typeof(int)));
            }
            else if (IsNumericDecimalType(typeValue))
            {
                constant = Expression.Constant(Convert.ChangeType(value, typeValue));
            }
            else if (typeValue.Name.Contains("List"))
            {
                if (operatorFilter == "@=" || operatorFilter == "!@=")
                    constant = Expression.Constant(Convert.ChangeType(value, GetTypeList(typeValue)));
                else
                    constant = Expression.Constant(Int32.Parse(value));
            }
            else
            {
                constant = Expression.Constant(Convert.ChangeType(value, typeValue));
            }
        }
        catch (Exception)
        {
            constant = null;
        }

        return constant;
    }

    /// <summary>
    /// Author:   Edwin Ibarra
    /// Create Date: 02/04/2024
    /// Retorna el tipo de la lista
    /// </summary>
    /// <param name="typeValue"></param>
    /// <returns></returns>
    public static Type GetTypeList(Type typeValue)
    {
        Type call;
        typeValue = typeValueNotNull(typeValue);
        switch (typeValue)
        {
            case Type t when t == typeof(List<int>):
                call = typeof(int);
                break;
            case Type t when t == typeof(List<decimal>):
                call = typeof(decimal);
                break;
            case Type t when t == typeof(List<double>):
                call = typeof(double);
                break;
            case Type t when t == typeof(List<float>):
                call = typeof(float);
                break;
            case Type t when t == typeof(List<long>):
                call = typeof(long);
                break;
            case Type t when t == typeof(List<short>):
                call = typeof(short);
                break;
            case Type t when t == typeof(List<byte>):
                call = typeof(byte);
                break;
            case Type t when t == typeof(List<bool>):
                call = typeof(bool);
                break;
            case Type t when t == typeof(List<DateTime>):
                call = typeof(DateTime);
                break;
            default:
                call = typeof(string);
                break;
        }
        return call;
    }

    /// <summary>
    /// Author:   Edwin Ibarra
    /// Date: 10/04/2024
    /// Valida si un tipo es númerico, se debe hacer esta validación porque los números que llegan con valor decimal con "." se debe cambiar el "." por ","
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsNumericDecimalType(Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Decimal:
            case TypeCode.Double:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Author:   Edwin Ibarra
    /// Date: 14/03/2024
    /// Quita el nullable de un tipo de dato y retorna el tipo de dato
    /// </summary>
    /// <param name="typeValue"></param>
    /// <returns></returns>
    public static Type typeValueNotNull(Type typeValue)
    {
           return Nullable.GetUnderlyingType(typeValue) ?? typeValue;
    }
}
