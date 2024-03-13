using System.Linq.Expressions;

namespace FilterSort.Helpers
{
    public class GenerateDinamicMethodCallExpression<T>
    {
        private static List<T> GetList(List<string> values)
        {
            List<T> list = new List<T>();
            foreach (var item in values)
            {
                list.Add((T)Convert.ChangeType(item, typeof(T)));
            }
            return list;
        }

        public static MethodCallExpression methodCall(Expression property, List<string> values)
        {
            var constant = Expression.Constant(GetList(values), typeof(List<T>));
            var method = typeof(List<T>).GetMethod("Contains", new[] { typeof(T) });
            var call = Expression.Call(constant, method, property);
            return call;
        }

    }
}
