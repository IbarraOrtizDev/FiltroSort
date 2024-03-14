using System.Linq.Expressions;
using FilterSort.Helpers;

namespace FilterSort
{
    public class FilterCondition
    {
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

        public static BinaryExpression BinaryExpression(List<string> listProperties, ParameterExpression parameter, string value)
        {
            BinaryExpression binaryExpressionsReturn = null;
            Expression constant = GenerateExpressionConstant.GetExpressionConstant("@=", value, null, typeof(string));
            foreach (var property in listProperties)
            {
                var propertyExp = Expression.Property(parameter, property);
                MethodCallExpression callExpression = Expression.Call(propertyExp, "Contains", null, constant);
                var expValidate = Expression.Equal(callExpression, Expression.Constant(true));
                if(binaryExpressionsReturn == null)
                    binaryExpressionsReturn = expValidate;
                else
                    binaryExpressionsReturn = Expression.OrElse(binaryExpressionsReturn, expValidate);
            }
            return binaryExpressionsReturn;
        }

        private static BinaryExpression resolveContains(Expression property, Type typeValue, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "Contains", null, constant);
            return Expression.Equal(callExpression, Expression.Constant(true));
        }
        private static BinaryExpression resolveNotContains(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "Contains", null, constant);
            var notContains = Expression.Not(callExpression);
            return Expression.Equal(notContains, Expression.Constant(true));
        }
        private static BinaryExpression resolveStartWith(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "StartsWith", null, constant);
            return Expression.Equal(callExpression, Expression.Constant(true));
        }
        private static BinaryExpression resolveNotStartWith(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "StartsWith", null, constant);
            var notStartWith = Expression.Not(callExpression);
            return Expression.Equal(notStartWith, Expression.Constant(true));
        }
        private static BinaryExpression resolveEndWith(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "EndsWith", null, constant);
            return Expression.Equal(callExpression, Expression.Constant(true));
        }
        private static BinaryExpression resolveNotEndWith(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "EndsWith", null, constant);
            var notEndWith = Expression.Not(callExpression);
            return Expression.Equal(notEndWith, Expression.Constant(true));
        }
        private static BinaryExpression resolveContainsCaseInsensitive(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "Contains", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            return Expression.Equal(callExpression, Expression.Constant(true));
        }
        private static BinaryExpression resolveStartWithCaseInsensitive(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "StartsWith", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            return Expression.Equal(callExpression, Expression.Constant(true));
        }
        private static BinaryExpression resolveEndWithCaseInsensitive(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "EndsWith", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            return Expression.Equal(callExpression, Expression.Constant(true));
        }
        private static BinaryExpression resolveEqualsCaseInsensitive(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "Equals", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            return Expression.Equal(callExpression, Expression.Constant(true));
        }
        private static BinaryExpression resolveNotEqualCaseInsensitive(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "Equals", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            var notEquals = Expression.Not(callExpression);
            return Expression.Equal(notEquals, Expression.Constant(true));
        }
        private static BinaryExpression resolveNotContainsCaseInsensitive(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "Contains", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            var notContains = Expression.Not(callExpression);
            return Expression.Equal(notContains, Expression.Constant(true));
        }
        private static BinaryExpression resolveNotStartWithCaseInsensitive(Expression property, Expression constant)
        {
            MethodCallExpression callExpression = Expression.Call(property, "StartsWith", null, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            var notStartWith = Expression.Not(callExpression);
            return Expression.Equal(notStartWith, Expression.Constant(true));
        }

        private static BinaryExpression resolveInOrNotIn(Expression property, List<string> values, Type typeValue, bool isIn)
        {
            MethodCallExpression call = resolveContainsMethod(property, values, typeValue);
            
            if (isIn)
                return Expression.Equal(call, Expression.Constant(true));
            var notStartWith = Expression.Not(call);
            return Expression.NotEqual(notStartWith, Expression.Constant(true));
        }

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
