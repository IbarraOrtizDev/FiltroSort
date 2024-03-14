using System.Linq.Expressions;

namespace FilterSort.Helpers
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

            if (values != null && values.Count > 0)
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
                "_=" => resolveGeneric(property, constant, "StartsWith"),
                "!_=" => resolveGenericNegative(property, constant, "StartsWith"),
                "_-=" => resolveGeneric(property, constant, "EndsWith"),
                "!_-=" => resolveGenericNegative(property, constant, "EndsWith"),
                "@=" => resolveGeneric(property, constant, "Contains"),
                "!@=" => resolveGenericNegative(property, constant, "Contains"),
                "@=*" => resolveGeneric(property, constant, "Contains", true),
                "_=*" => resolveGeneric(property, constant, "StartsWith", true),
                "_-=*" => resolveGeneric(property, constant, "EndsWith",true),
                "==*" => resolveGeneric(property, constant, "Equals", true),
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
        ///    Description: Se encarga de generar el BinaryExpression, el metodo tipo de metodo a utilizar se recibe por parametro y de acuerdo al parametro ignoreCase se determina si lo ignora o no
        /// </summary>
        /// <param name="property"></param>
        /// <param name="constant"></param>
        /// <param name="method"></param>
        /// <param name="ignoreCase"></param>
        /// <returns>
        /// Retorna la expresion binaria
        /// </returns>
        private static BinaryExpression resolveGeneric(Expression property, Expression constant, string method, bool ignoreCase = false)
        {
            MethodCallExpression callExpression;
            if (!ignoreCase)
                callExpression = Expression.Call(property, method, null, constant);
            else
                callExpression = Expression.Call(property, method, null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            return Expression.Equal(callExpression, Expression.Constant(true));
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
        private static BinaryExpression resolveGenericNegative(Expression property, Expression constant, string method, bool ignoreCase = false)
        {
            MethodCallExpression callExpression;
            if (!ignoreCase)
                callExpression = Expression.Call(property, method, null, constant);
            else
                callExpression = Expression.Call(property, method, null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
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
    }
}
