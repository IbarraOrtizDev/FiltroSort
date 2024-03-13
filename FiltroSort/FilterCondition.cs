using System.Linq.Expressions;
using System.Reflection.Metadata;

namespace FilterSort
{
    public class FilterCondition
    {
        public static BinaryExpression BinaryExpression(string propertyName, string operatorFilter, ParameterExpression parameter, string value, List<string> values, Type typeValue)
        {
            var property = Expression.Property(parameter, propertyName);
            Expression constant;

            try
            {
                if (typeValue == typeof(string))
                {
                    constant = Expression.Constant(value);
                }
                else
                {
                    constant = Expression.Constant(Convert.ChangeType(value, typeValue));
                }
            }
            catch (Exception ex)
            {
                return null;
            }

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
                _ => throw new ArgumentException("Invalid operator filter")
            };
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

    }
}
