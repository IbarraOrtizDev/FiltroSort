using FiltroSort;

namespace FiltroSortApi.DTOs
{
    public class DataDTO
    {
        [Searchable]
        public string propiedadString { get; set; } = string.Empty;
        [Searchable]
        public bool propiedadBooleana { get; set; }
        [Searchable]
        public int propiedadEntera { get; set; }
        [Searchable]
        public DateTime propiedadFecha
        {
            get;
            set;
        }
    }
}
