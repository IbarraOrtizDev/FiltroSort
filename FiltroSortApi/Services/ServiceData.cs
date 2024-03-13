using FiltroSortApi.DTOs;
using Newtonsoft.Json;

namespace FiltroSortApi.Services
{
    public class ServiceData
    {
        List<DataDTO> _data;

        public ServiceData()
        {
            var dataFile = "./SeedData/dataFilter.json";
            var data = File.ReadAllText(dataFile);
            _data = JsonConvert.DeserializeObject<List<DataDTO>>(data);
        }

        public List<DataDTO> GetData()
        {
            return _data;
        }

    }
}
