using System.Linq.Expressions;

namespace FilterSort.Helpers
{
    /// <summary>
    ///    Author:   Edwin Ibarra
    ///    Create Date: 14/03/2024
    ///     Generate Expression Constant, esta clase se encarga de generar la expresion constante
    /// </summary>
    public class GenerateExpressionConstant
    {
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
        public static Expression GetExpressionConstant(string operatorFilter, string value, List<string> values, Type typeValue)
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
                        constant = Expression.Constant(value);
                    }
                    else
                    {
                        constant = Expression.Constant(Convert.ChangeType(value, typeValue));
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
}
