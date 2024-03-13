namespace FiltroSortApi.DTOs
{
    public class DataDTO
    {
        public string propiedadString { get; set; } = string.Empty;
        public bool propiedadBooleana { get; set; }
        public int propiedadEntera { get; set; }

        public DateTime propiedadFecha
        {
            get;
            set;
        }
    }
}
