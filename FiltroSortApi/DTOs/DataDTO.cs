using FiltroSort;

namespace FiltroSortApi.DTOs
{
    public class DataDTO
    {
        [Searchable]
        public EnumTestType type { get; set; }
        [Searchable]
        public string? propiedadString { get; set; } = string.Empty;
        [Searchable]
        public bool? propiedadBooleana { get; set; }
        [Searchable]
        public int propiedadEntera { get; set; }
        [Searchable]
        public DateTime? propiedadFecha
        {
            get;
            set;
        }
        [Searchable]
        public List<string>? propiedadListaString { get; set;}
        [Searchable]
        public List<int>? propiedadListaEntera { get; set; }
        [Searchable]
        public PropiedadObjeto? propiedadObjeto { get; set; }
        [Searchable]
        public List<PropiedadObjeto> propiedadListaObjetos { get; set; }
    }
    public class PropiedadObjeto
    {
        [Searchable]
        public string? propiedadString { get; set; } = string.Empty;
        [Searchable]
        public bool? propiedadBooleana { get; set; }
        [Searchable]
        public int propiedadEntera { get; set; }
        [Searchable]
        public DateTime? propiedadFecha
        {
            get;
            set;
        }
        [Searchable]
        public List<string>? propiedadListaString { get; set; }
        [Searchable]
        public List<int>? propiedadListaEntera { get; set; }
        [Searchable]
        public List<PropiedadObjeto2>? propiedadListaObjetos { get; set; }
    }

    public class PropiedadObjeto2
    {
        [Searchable]
        public string? propiedadString { get; set; } = string.Empty;
        [Searchable]
        public bool? propiedadBooleana { get; set; }
        [Searchable]
        public int propiedadEntera { get; set; }
        [Searchable]
        public DateTime? propiedadFecha
        {
            get;
            set;
        }
        [Searchable]
        public List<string>? propiedadListaString { get; set; }
        [Searchable]
        public List<int>? propiedadListaEntera { get; set; }
    }
}
