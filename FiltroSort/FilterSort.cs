using System.Linq.Expressions;
using FilterSort;
using FilterSort.Models;

namespace FiltroSort
{
    public class FilterSort<T>
    {
        FilterSoftModel _modelFiltrol;

        public FilterSort(FilterSoftModel modelFiltrol)
        {
            _modelFiltrol = modelFiltrol;
        }

        public Expression<Func<T, bool>> GetFilterExpression()
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var expression = GetFilterExpression(parameter, _modelFiltrol.Filter);
            if(expression == null) return x => true;
            //Pagination
            return Expression.Lambda<Func<T, bool>>(expression, parameter);
        }
        private BinaryExpression GetFilterExpression(ParameterExpression parameter, string filterParam)
        {
            if (string.IsNullOrWhiteSpace(filterParam)) return null;

            //List properties of T
            var properties = typeof(T).GetProperties();
            var propertiesList = properties.Select(x => x.Name).ToList();

            var listFilters = filterParam.Split(',').ToList();
            BinaryExpression binaryExpressions = null;

            foreach (var filter in listFilters)
            {
                var conditionData = GetConditionExpression(filter);
                if(conditionData == null || (conditionData.Value == null && conditionData.Values == null)) continue;
                if (conditionData == null || !propertiesList.Contains(conditionData.PropertyName)) continue;
                var property = typeof(T).GetProperty(conditionData.PropertyName);
                var condition = FilterCondition.BinaryExpression(conditionData.PropertyName, conditionData.Operator, parameter,  conditionData.Value, conditionData.Values, property.PropertyType);
                if(condition == null) continue;
                if (binaryExpressions == null)
                {
                    binaryExpressions = condition;
                }
                else
                {
                    binaryExpressions = Expression.AndAlso(binaryExpressions, condition);
                }
            }

            return binaryExpressions;
        }

        private DeserializeFilterProperty GetConditionExpression(string filterParamUnique)
        {
            var listOperators = new List<string> { "!_=*", "!@=*", "_-=*", "!_-=", "_-=", "!@=", "!_=", "@=*", "_=*", "==*", "!=*", "==", "!=", ">=", "<=", "@=", "_=", ">", "<" };

            string operatorFilter = string.Empty;

            foreach (var item in listOperators)
            {
                if (filterParamUnique.Contains(item))
                {
                    operatorFilter = item;
                    break;
                }
            }
            if(operatorFilter == string.Empty) return null;
            if(typeof(T).GetProperty(filterParamUnique.Split(operatorFilter)[0]) == null || !OperatorIsValidForType(operatorFilter, typeof(T).GetProperty(filterParamUnique.Split(operatorFilter)[0]).PropertyType)) return null;
            DeserializeFilterProperty deserializeFilterProperty = new DeserializeFilterProperty();
            deserializeFilterProperty.Operator = operatorFilter;
            deserializeFilterProperty.PropertyName = filterParamUnique.Split(operatorFilter)[0];
            var valor = filterParamUnique.Split(operatorFilter)[1];
            if(!(valor == null || valor == string.Empty))
            {
                if (valor.Contains("|"))
                {
                    deserializeFilterProperty.Values = valor.Split('|').ToList();
                }
                else
                {
                    deserializeFilterProperty.Value = valor;
                }

            }
            return deserializeFilterProperty;
        }
        private bool OperatorIsValidForType(string operatorFilter, Type type)
        {
            if (type == typeof(string))
            {
                return operatorFilter switch
                {
                    "==" => true,
                    "!=" => true,
                    "@=" => true,
                    "_=" => true,
                    "_-=" => true,
                    "!@=" => true,
                    "!_=" => true,
                    "!_-=" => true,
                    "@=*" => true,
                    "_=*" => true,
                    "_-=*" => true,
                    "==*" => true,
                    "!=*" => true,
                    "!@=*" => true,
                    "!_=*" => true,
                    _ => false
                };
            }
            else if (type == typeof(int) || type == typeof(double) || type == typeof(decimal))
            {
                return operatorFilter switch
                {
                    "==" => true,
                    "!=" => true,
                    ">" => true,
                    "<" => true,
                    ">=" => true,
                    "<=" => true,
                    _ => false
                };
            }
            else if (type == typeof(DateTime))
            {
                return operatorFilter switch
                {
                    "==" => true,
                    "!=" => true,
                    ">" => true,
                    "<" => true,
                    ">=" => true,
                    "<=" => true,
                    _ => false
                };
            }
            else if (type == typeof(bool))
            {
                return operatorFilter switch
                {
                    "==" => true,
                    "!=" => true,
                    _ => false
                };
            }
            return false;
        }   
    }
}
