using Microsoft.AspNetCore.Mvc;
using FiltroSortApi.DTOs;
using FiltroSortApi.Services;
using FilterSort.Models;
using FiltroSort;
using System.Linq.Expressions;
namespace FiltroSortApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        ServiceData _serviceData;
        public ValuesController()
        {
            _serviceData = new ServiceData();
        }
        // GET: api/Values
        [HttpGet]
        public IActionResult Get([FromQuery] FilterSoftModel dataQuery)
        {
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(dataQuery);
            var filt = filterSort.GetFilterExpression();
            //var data = _serviceData.GetData().AsQueryable().Where(filt).ToList();
            var page = dataQuery.Page??1;
            var pageSize = dataQuery.PageSize??10;
            IQueryable<DataDTO> query = _serviceData.GetData().AsQueryable();
            if(filt != null)
            {
                query = query.Where(filt);
            }

            if(!string.IsNullOrEmpty(dataQuery.Sort))
            {
                var type = dataQuery.Sort.Split(":")[0];
                if (type != "asc" && type != "desc")
                {
                    return BadRequest("El tipo de ordenamiento no es válido");
                }

                var propertyName = dataQuery.Sort.Split(":")[1];
                var property = typeof(DataDTO).GetProperty(propertyName);

                if (property == null)
                {
                    return BadRequest("La propiedad de ordenamiento no existe");
                }

                Expression<Func<DataDTO, object>> expression = x => property.GetValue(x);

                if (type == "asc")
                {
                    query = query.OrderBy(expression);

                }
                else
                {
                    query = query.OrderByDescending(expression);
                }
            }

            if(dataQuery.Page.HasValue && dataQuery.PageSize.HasValue)
            {
                query = query.Skip((page-1)*pageSize).Take(pageSize);
            }
            var data = query.ToList();
            return Ok(data);
        }
    }
}
