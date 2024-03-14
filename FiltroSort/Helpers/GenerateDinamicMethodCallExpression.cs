using System.Linq.Expressions;

namespace FilterSort.Helpers
{
    /// <summary>
    /// Generate Dinamic Method Call Expression, esta clase se encarga de generar la expresion de llamada a metodo dinamica, el objetivo es utilizala para generar la expresion de llamada a metodo Contains, cuando desde el filtro se envian los "|" para indicar que se debe buscar en una lista de valores
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenerateDinamicMethodCallExpression<T>
    {
        /// <summary>
        /// Get Method Call, este metodo se encarga de convertir la lista de valores a una lista de tipo T
        /// </summary>
        /// <param name="values"></param>
        /// <returns>
        /// List<T> lista de valores de tipo T
        /// </returns>
        private static List<T> GetList(List<string> values)
        {
            List<T> list = new List<T>();
            foreach (var item in values)
            {
                list.Add((T)Convert.ChangeType(item, typeof(T)));
            }
            return list;
        }
        /// <summary>
        /// Method Call, este metodo se encarga de generar la expresion de llamada a metodo
        /// </summary>
        /// <param name="property"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static MethodCallExpression methodCall(Expression property, List<string> values)
        {
            var constant = Expression.Constant(GetList(values), typeof(List<T>));
            var method = typeof(List<T>).GetMethod("Contains", new[] { typeof(T) });
            var call = Expression.Call(constant, method, property);
            return call;
        }

    }
}
