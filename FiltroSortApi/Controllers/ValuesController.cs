using Microsoft.AspNetCore.Mvc;
using FiltroSortApi.DTOs;
using FiltroSortApi.Services;
using FilterSort.Models;
using FiltroSort;
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
            var data = _serviceData.GetData().AsQueryable().Where(filt).Skip((page-1)*pageSize).Take(pageSize);
            return Ok(data);
        }
    }
}
