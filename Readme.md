# FiltroSort

FiltroSort busca ser una herramienta que permita a los usuarios filtrar y visualizar información de manera sencilla y rápida. La idea es que el usuario pueda cargar un archivo de datos, seleccionar las columnas que le interesan y aplicar filtros a los datos, ordenarlos y paginarlos.

Basicamente FilterSoft recibe como parametro un objeto con la siguiente estructura:

```C#
{
	Filters: "",
}
```

# Documentación

## Operadores de filtro

| Operator | Meaning                                      |
| -------- | -------------------------------------------- |
| `==`     | Equals                                       |
| `!=`     | Not equals                                   |
| `>`      | Greater than                                 |
| `<`      | Less than                                    |
| `>=`     | Greater than or equal to                     |
| `<=`     | Less than or equal to                        |
| `@=`     | Contains                                     |
| `_=`     | Starts with                                  |
| `_-=`    | Ends with                                    |
| `!@=`    | Does not Contains                            |
| `!_=`    | Does not Starts with                         |
| `!_-=`   | Does not Ends with                           |
| `@=*`    | Case-insensitive string Contains             |
| `_=*`    | Case-insensitive string Starts with          |
| `_-=*`   | Case-insensitive string Ends with            |
| `==*`    | Case-insensitive string Equals               |
| `!=*`    | Case-insensitive string Not equals           |
| `!@=*`   | Case-insensitive string does not Contains    |
| `!_=*`   | Case-insensitive string does not Starts with |

## Nota :

filter=Alex

1. Si el operador no es valido y solo se tiene una propiedad filtrable en el queryparam, la sección del filtro sera tomada como un valor a buscar en todas las propiedades [Searchable], de lo contrario sera ignorada.
2. Si el tipo de dato del filtro no corresponde al tipo de dato de la propiedad, este sera ignorado.
3. Si la clave valor que se pase en el filtro no tiene valor, este sera ignorado.
4. Los operadores no son aplicables a todos los tipo de datos.
5. Si el filtro es vacio, se retornara una lambda que siempre retorna true.
6. Para buscar en todas las propiedades [Searchable] se debe pasar el filtro como un valor sin operador.

## Los operadores aplicables a los tipos de datos son los siguientes:

| Type                                     | Operator                                                                                                                                                                              |
| ---------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `string`                                 | `==`, `>`, `<=`, `>`, `>=`, `!=`, `@=`, `_=`, `_-=`, `!@=`, `!_=`, `!_-=`, `@=*`, `_=*`, `_-=*`, `==*`, `!=*`, `!@=*`, `!_=*` Los operadores [`>`, `<=`, `>`, `>=`] permiten evaluar la longitud de un texto|
| `int`                                    | `==`, `!=`, `>`, `<`, `>=`, `<=`                                                                                                                                                      |
| `double`                                 | `==`, `!=`, `>`, `<`, `>=`, `<=`                                                                                                                                                      |
| `decimal`                                | `==`, `!=`, `>`, `<`, `>=`, `<=`                                                                                                                                                      |
| `DateTime`                               | `==`, `!=`, `>`, `<`, `>=`, `<=`                                                                                                                                                      |
| `bool`                                   | `==`, `!=`                                                                                                                                                                            |
| `List<string>,List<int> (tipos basicos)` | `@=`, `!@=`,`<`, `<=`,`>`, `>=` Se utiliza cuando se quiere evaluar los valores de una lista, tener en cuenta que los operadores >, >=, < y <= aplican solo para el Count de la lista |

## Ejemplo de filtro

```C#
var filter = "propertyName==value,propertyAge>=20,city==Medellín|Bogota"
//Example response
// var cities = new List<string> { "Medellín", "Bogota" };
//.Where(x => x.propertyName == "value" && x.propertyAge >= 20 && cities.Contains(x.city))
```

## Si quiero filtrar por todas las categorias Searchable

```C#
var filter = "valor"
//Con esto ya se encarga de buscar ese valor con el metodo .Contains en todas las propiedades searchable
```

## Consulta por grupo de filtros de una propiedad

Cuando se requiere filtrar por un grupo de filtros de una propiedad, se puede hacer uso de la siguiente sintaxis:

```C#
var filter = "propertyName==value1|value2|value3"
//En este caso se busca que la propiedad propertyName sea igual a value1, value2 o value3 y se puede extender a n valores, ademas tener en cuenta que el operador | es un OR y no un AND y adicionalmente se pueden utilizar los operadores correspondientes a los tipos de datos.
//Example response
//.Where(x => (x.propertyName == "value1" || x.propertyName == "value2" || x.propertyName == "value3"))
```

El resultado del filtro es una expresión lambda que se puede aplicar a una lista de objetos.

## Ejemplo de implementación

```C#
using FilterSoft;
string filter = "propertyName==value,propertyAge>=20,city@=Medellín|Bogota";
var lista = new List<YourClass>();
FilterSoft<YourClass> filterSoft = new FilterSoft<YourClass>();
var filter = filterSoft.GetFilterExpression(filter);
lista.Where(filter).ToList();
```

## Buscar un elemento en una lista de datos

```C#
using FilterSoft;
string filter = "propertyName@=value1";
//propertyName es una lista de string
var lista = new List<YourClass>();
FilterSoft<YourClass> filterSoft = new FilterSoft<YourClass>();
var filter = filterSoft.GetFilterExpression(filter);
lista.Where(filter).ToList();
```

## Buscar un valor nulo en una propiedad, se puede utilizar el operador ==null o !=null

```C#
using FilterSoft;
string filter = "propertyName==null";
var lista = new List<YourClass>();
FilterSoft<YourClass> filterSoft = new FilterSoft<YourClass>();
var filter = filterSoft.GetFilterExpression(filter);
lista.Where(filter).ToList();
```

## Buscar en Objetos anidados

Cuando hablamos de objetos anidados contemplamos las siguientes opciones:

```C#
class ClasePrincipal {
	public string property {get;set;}
	//Las siguientes opciones corresponden a los objetos anidados:
	[Searchable]
	public ClaseSecundaria properyObject {get;set;}
	[Searchable]
	public List<ClaseSecundaria> propertyListObject {get;set;}
}
class ClaseSecundaria {
	[Searchable]
	public string propertyOne {get;set;}
	[Searchable]
	public int propertyTwo {get;set;}
	[Searchable]
	public bool propertyThree {get;set;}
	[Searchable]
	public List<string> propertyFour {get;set;}
}
```

De acuerdo con el ejemplo anterio, las consultas se pueden realizar de la siguiente manera:

```C#
using FilterSoft;
string filter = "properyObject.propertyOne==Example";
var lista = new List<YourClass>();
FilterSoft<YourClass> filterSoft = new FilterSoft<YourClass>();
var filter = filterSoft.GetFilterExpression(filter);
// Esto es similar al siguiente lambda: x=> x.propertyObject?.propertyOne=='Example'
lista.Where(filter).ToList();
```

Si se quiere buscar en una lista de objetos:

```C#
using FilterSoft;
string filter = "propertyListObject.propertyOne==Example";
var lista = new List<YourClass>();
FilterSoft<YourClass> filterSoft = new FilterSoft<YourClass>();
var filter = filterSoft.GetFilterExpression(filter);
// Esto es similar al siguiente lambda: x=> x.propertyObject !== null && x.Any(y=> y.propertyOne=='Example'
lista.Where(filter).ToList();
```

Evaluar la longitud de un texto
```C#
using FilterSoft;
string filter = "propertyString>=5";
var lista = new List<YourClass>();
FilterSoft<YourClass> filterSoft = new FilterSoft<YourClass>();
var filter = filterSoft.GetFilterExpression(filter);
// Esto es similar al siguiente lambda: x=> x.propertyString !== null && x.propertyString >= 5
lista.Where(filter).ToList();
```

Consultar elementos de texto vacios
```C#
using FilterSoft;
string filter = "propertyString==";
var lista = new List<YourClass>();
FilterSoft<YourClass> filterSoft = new FilterSoft<YourClass>();
var filter = filterSoft.GetFilterExpression(filter);
// Esto es similar al siguiente lambda: x=> x.propertyString !== null && x.propertyString == ""
lista.Where(filter).ToList();
```