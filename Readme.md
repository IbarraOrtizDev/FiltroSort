﻿# FilterSoft

FilterSoft busca ser una herramienta que permita a los usuarios filtrar y visualizar información de manera sencilla y rápida. La idea es que el usuario pueda cargar un archivo de datos, seleccionar las columnas que le interesan y aplicar filtros a los datos, ordenarlos y paginarlos.

Basicamente FilterSoft recibe como parametro un objeto con la siguiente estructura:

``` C#
{
	Filters: "",
	OrderBy: "",
	Page: 1,
	PageSize: 10
}
```

# Documentación

## Operadores de filtro
| Operator   | Meaning                  |
|------------|--------------------------|
| `==`       | Equals                   |
| `!=`       | Not equals               |
| `>`        | Greater than             |
| `<`        | Less than                |
| `>=`       | Greater than or equal to |
| `<=`       | Less than or equal to    |
| `@=`       | Contains                 |
| `_=`       | Starts with              |
| `_-=`      | Ends with                |
| `!@=`      | Does not Contains        |
| `!_=`      | Does not Starts with     |
| `!_-=`     | Does not Ends with       |
| `@=*`      | Case-insensitive string Contains |
| `_=*`      | Case-insensitive string Starts with |
| `_-=*`     | Case-insensitive string Ends with |
| `==*`      | Case-insensitive string Equals |
| `!=*`      | Case-insensitive string Not equals |
| `!@=*`     | Case-insensitive string does not Contains |
| `!_=*`     | Case-insensitive string does not Starts with |

## Nota : 
1.	Si el operador no es valido sera ignorado.
2.	Si el tipo de dato del filtro no corresponde al tipo de dato de la propiedad, este sera ignorado.
3.	Si la clave valor que se pase en el filtro no tiene valor, este sera ignorado.
4.	Los operadores no son aplicables a todos los tipo de datos.

## Los operadores aplicables a los tipos de datos son los siguientes:

| Type       | Operator   |
|------------|------------|
| `string`   | `==`, `!=`, `@=`, `_=`, `_-=`, `!@=`, `!_=`, `!_-=`, `@=*`, `_=*`, `_-=*`, `==*`, `!=*`, `!@=*`, `!_=*` |
| `int`      | `==`, `!=`, `>`, `<`, `>=`, `<=` |
| `double`   | `==`, `!=`, `>`, `<`, `>=`, `<=` |
| `decimal`  | `==`, `!=`, `>`, `<`, `>=`, `<=` |
| `DateTime` | `==`, `!=`, `>`, `<`, `>=`, `<=` |
| `bool`     | `==`, `!=` |

## Ejemplo de filtro

``` C#
var filter = "propertyName==value,propertyAge>=20,city@=Medellín|Bogota"
//Example response 
// var cities = new List<string> { "Medellín", "Bogota" };
//.Where(x => x.propertyName == "value" && x.propertyAge >= 20 && cities.Contains(x.city))
```

## Nota: Tener en cuenta que el operador que no sea valido sera ignorado y esto es aplicado a nivel de operador y de operación.

El resultado del filtro es una expresión lambda que se puede aplicar a una lista de objetos.

## Ejemplo de implementación

``` C#
using FilterSoft;
string filter = "propertyName==value,propertyAge>=20,city@=Medellín|Bogota";
var lista = new List<YourClass>();
FilterSoft<YourClass> filterSoft = new FilterSoft<YourClass>();
var filter = filterSoft.GetFilterExpression(filter);
lista.Where(filter).ToList();
```
