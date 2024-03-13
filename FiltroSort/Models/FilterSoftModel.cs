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
    /// </summary>
    public string Sort { get; set; } = string.Empty;

    /// <summary>
    /// Establece si el ordenamiento debe ser ascendente o descendente
    /// </summary>
    public OrderEnum Order { get; set; } = OrderEnum.Asc;

    /// <summary>
    /// Establece el número de página
    /// </summary>
    public int? Page { get; set; } = 1;

    /// <summary>
    /// Establece el número de elementos por página
    /// </summary>
    public int? PageSize { get; set; } = 10;


}
