using FiltroSort;

namespace FilterSort.Models;

public class FilterSoftModel
{
    /// <summary>
    /// Filtro de búsqueda
    /// </summary>
    public string Filter { get; set; } = string.Empty;

    /// <summary>
    ///  Establece la propiedad por la que se debe ordenar  
    ///  Se debe establecer de la siguiente forma: desc:nombrePropiedad o asc:nombrePropiedad
    /// </summary>
    public string Sort { get; set; } = string.Empty;


    /// <summary>
    /// Establece el número de página
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    /// Establece el número de elementos por página
    /// </summary>
    public int? PageSize { get; set; }


}
