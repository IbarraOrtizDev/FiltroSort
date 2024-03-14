using System.Linq.Expressions;
using FilterSort.Helpers;

namespace FilterSort
{
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
        public static BinaryExpression BinaryExpression(string propertyName, string operatorFilter, ParameterExpression parameter, string value, List<string> values, Type typeValue)
        {
            var property = Expression.Property(parameter, propertyName);
            
            if(values != null && values.Count > 0)
            {
                operatorFilter = operatorFilter == "!@=" ? "NOT IN" : "IN";
            }
            Expression constant = GenerateExpressionConstant.GetExpressionConstant(operatorFilter, value, values, typeValue);

            if (constant == null) return null;

            return operatorFilter switch
            {
                "==" => Expression.Equal(property, constant),
                "!=" => Expression.NotEqual(property, constant),
                ">" => Expression.GreaterThan(property, constant),
                "<" => Expression.LessThan(property, constant),
                ">=" => Expression.GreaterThanOrEqual(property, constant),
                "<=" => Expression.LessThanOrEqual(property, constant),
                "_=" => resolveStartWith(property, constant),
                "!_=" => resolveNotStartWith(property, constant),
                "_-=" => resolveEndWith(property, constant),
                "!_-=" => resolveNotEndWith(property, constant),
                "@=" => resolveContains(property, typeValue, constant),
                "!@=" => resolveNotContains(property, constant),
                "@=*" => resolveContainsCaseInsensitive(property, constant),
                "_=*" => resolveStartWithCaseInsensitive(property, constant),
                "_-=*" => resolveEndWithCaseInsensitive(property, constant),
                "==*" => resolveEqualsCaseInsensitive(property, constant),
                "!=*" => resolveNotEqualCaseInsensitive(property, constant),
                "!@=*" => resolveNotContainsCaseInsensitive(property, constant),
                "!_=*" => resolveNotStartWithCaseInsensitive(property, constant),
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
        public static BinaryExpression BinaryExpression(List<string> listProperties, ParameterExpression parameter, string value)
        {
            BinaryExpression binaryExpressionsReturn = null;
            Expression constant = GenerateExpressionConstant.GetExpressionConstant("@=", value, null, typeof(string));
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
        ///    resolveContains, este metodo se encarga de generar la expresion binaria para el operador Contains
        /// </summary>
        /// <param name="property"></param>
        /// <param name="typeValue"></param>
        /// <param name="constant"></param>
        /// <returns>
        /// Retorna la expresion binaria de Contains
        /// </returns>
        private static BinaryExpression resolveContains(Expression property, Type typeValue, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "Contains", null, constant);
            return Expression.Equal(callExpression, Expression.Constant(true));
        }

        /// <summary>
        ///    Author:   Edwin Ibarra
        ///    Create Date: 14/03/2024
        ///    resolveNotContains, este metodo se encarga de generar la expresion binaria para el operador Not Contains
        /// </summary>
        /// <param name="property"></param>
        /// <param name="constant"></param>
        /// <returns>
        /// Retorna la expresion binaria de Not Contains
        /// </returns>
        private static BinaryExpression resolveNotContains(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "Contains", null, constant);
            var notContains = Expression.Not(callExpression);
            return Expression.Equal(notContains, Expression.Constant(true));
        }

        /// <summary>
        ///    Author:   Edwin Ibarra
        ///    Create Date: 14/03/2024
        ///    resolveStartWith, este metodo se encarga de generar la expresion binaria para el operador Start With
        /// </summary>
        /// <param name="property"></param>
        /// <param name="constant"></param>
        /// <returns>
        /// Retorna la expresion binaria de Start With
        /// </returns>
        private static BinaryExpression resolveStartWith(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "StartsWith", null, constant);
            return Expression.Equal(callExpression, Expression.Constant(true));
        }

        /// <summary>
        ///    Author:   Edwin Ibarra
        ///    Create Date: 14/03/2024
        ///    resolveNotStartWith, este metodo se encarga de generar la expresion binaria para el operador Not Start With
        /// </summary>
        /// <param name="property"></param>
        /// <param name="constant"></param>
        /// <returns>
        /// Retorna la expresion binaria de Not Start With
        /// </returns>
        private static BinaryExpression resolveNotStartWith(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "StartsWith", null, constant);
            var notStartWith = Expression.Not(callExpression);
            return Expression.Equal(notStartWith, Expression.Constant(true));
        }

        /// <summary>
        ///    Author:   Edwin Ibarra
        ///    Create Date: 14/03/2024
        ///    resolveEndWith, este metodo se encarga de generar la expresion binaria para el operador End With
        /// </summary>
        /// <param name="property"></param>
        /// <param name="constant"></param>
        /// <returns>
        /// Retorna la expresion binaria de End With
        /// </returns>
        private static BinaryExpression resolveEndWith(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "EndsWith", null, constant);
            return Expression.Equal(callExpression, Expression.Constant(true));
        }

        /// <summary>
        ///    Author:   Edwin Ibarra
        ///    Create Date: 14/03/2024
        ///    resolveNotEndWith, este metodo se encarga de generar la expresion binaria para el operador Not End With
        /// </summary>
        /// <param name="property"></param>
        /// <param name="constant"></param>
        /// <returns>
        /// Retorna la expresion binaria de Not End With
        /// </returns>
        private static BinaryExpression resolveNotEndWith(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "EndsWith", null, constant);
            var notEndWith = Expression.Not(callExpression);
            return Expression.Equal(notEndWith, Expression.Constant(true));
        }

        /// <summary>
        ///    Author:   Edwin Ibarra
        ///    Create Date: 14/03/2024
        ///    resolveContainsCaseInsensitive, este metodo se encarga de generar la expresion binaria para el operador Contains ignorando mayusculas y minusculas
        /// </summary>
        /// <param name="property"></param>
        /// <param name="constant"></param>
        /// <returns>
        /// Retorna la expresion binaria de Contains ignorando mayusculas y minusculas
        /// </returns>
        private static BinaryExpression resolveContainsCaseInsensitive(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "Contains", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            return Expression.Equal(callExpression, Expression.Constant(true));
        }

        /// <summary>
        ///    Author:   Edwin Ibarra
        ///    Create Date: 14/03/2024
        ///    resolveStartWithCaseInsensitive, este metodo se encarga de generar la expresion binaria para el operador Start With ignorando mayusculas y minusculas
        /// </summary>
        /// <param name="property"></param>
        /// <param name="constant"></param>
        /// <returns>
        /// Retorna la expresion binaria de Start With ignorando mayusculas y minusculas
        /// </returns>
        private static BinaryExpression resolveStartWithCaseInsensitive(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "StartsWith", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            return Expression.Equal(callExpression, Expression.Constant(true));
        }

        /// <summary>
        ///    Author:   Edwin Ibarra
        ///    Create Date: 14/03/2024
        ///    resolveEndWithCaseInsensitive, este metodo se encarga de generar la expresion binaria para el operador End With ignorando mayusculas y minusculas
        /// </summary>
        /// <param name="property"></param>
        /// <param name="constant"></param>
        /// <returns>
        /// Retorna la expresion binaria de End With ignorando mayusculas y minusculas
        /// </returns>
        private static BinaryExpression resolveEndWithCaseInsensitive(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "EndsWith", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            return Expression.Equal(callExpression, Expression.Constant(true));
        }

        /// <summary>
        ///    Author:   Edwin Ibarra
        ///    Create Date: 14/03/2024
        ///    resolveEqualsCaseInsensitive, este metodo se encarga de generar la expresion binaria para el operador Equals ignorando mayusculas y minusculas
        /// </summary>
        /// <param name="property"></param>
        /// <param name="constant"></param>
        /// <returns>
        /// Retorna la expresion binaria de Equals ignorando mayusculas y minusculas
        /// </returns>
        private static BinaryExpression resolveEqualsCaseInsensitive(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "Equals", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            return Expression.Equal(callExpression, Expression.Constant(true));
        }

        /// <summary>
        ///    Author:   Edwin Ibarra
        ///    Create Date: 14/03/2024
        ///    resolveNotEqualCaseInsensitive, este metodo se encarga de generar la expresion binaria para el operador Not Equal ignorando mayusculas y minusculas
        /// </summary>
        /// <param name="property"></param>
        /// <param name="constant"></param>
        /// <returns>
        /// Retorna la expresion binaria de Not Equal ignorando mayusculas y minusculas
        /// </returns>
        private static BinaryExpression resolveNotEqualCaseInsensitive(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "Equals", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            var notEquals = Expression.Not(callExpression);
            return Expression.Equal(notEquals, Expression.Constant(true));
        }

        /// <summary>
        ///    Author:   Edwin Ibarra
        ///    Create Date: 14/03/2024
        ///    resolveNotContainsCaseInsensitive, este metodo se encarga de generar la expresion binaria para el operador Not Contains ignorando mayusculas y minusculas
        /// </summary>
        /// <param name="property"></param>
        /// <param name="constant"></param>
        /// <returns>
        /// Retorna la expresion binaria de Not Contains ignorando mayusculas y minusculas
        /// </returns>
        private static BinaryExpression resolveNotContainsCaseInsensitive(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "Contains", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            var notContains = Expression.Not(callExpression);
            return Expression.Equal(notContains, Expression.Constant(true));
        }

        /// <summary>
        ///    Author:   Edwin Ibarra
        ///    Create Date: 14/03/2024
        ///    resolveNotStartWithCaseInsensitive, este metodo se encarga de generar la expresion binaria para el operador Not Start With ignorando mayusculas y minusculas
        /// </summary>
        /// <param name="property"></param>
        /// <param name="constant"></param>
        /// <returns></returns>
        private static BinaryExpression resolveNotStartWithCaseInsensitive(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "StartsWith", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            var notStartWith = Expression.Not(callExpression);
            return Expression.Equal(notStartWith, Expression.Constant(true));
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
    }
}
