using System.Linq.Expressions;
using FilterSort.Helpers;
using FilterSort.Models;

namespace FiltroSort
{
    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 14/03/2024
    ///    Clase que se encarga de generar la expresion lambda para filtrar
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FilterSort<T>
    {
        FilterSoftModel _modelFiltrol;

        public FilterSort(FilterSoftModel modelFiltrol)
        {
            _modelFiltrol = modelFiltrol;
        }

        /// <summary>
        ///    Author:   Edwin Ibarra
        ///    Create Date: 14/03/2024
        ///    Crea la lambda expression para filtrar los datos, de acuerdo a la cadena de filtro
        /// </summary>
        /// <returns>
        /// Expresion lambda a evaluar
        /// </returns>
        public Expression<Func<T, bool>> GetFilterExpression()
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var expression = GenerateBinaryExpression<T>.GetFilterExpressionGenerator(parameter, _modelFiltrol.Filter);
            if(expression == null) return x => true;
            //Pagination
            return Expression.Lambda<Func<T, bool>>(expression, parameter);
        }
    }
}
