using Gridify;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

public class GridQuery : IGridifyQuery
{
    public GridQuery() { }
    public GridQuery(int page, int pageSize, string filter, string? sort = null)
    {
        Page = page;
        PageSize = pageSize;
        OrderBy = sort;
        Filter = filter;
    }

    [PageDefaultValue]
    [PageRange]
    [FromQuery(Name = "page")]
    [Description("The page of the result.")]
    public int Page { get; set; }

    [PageSizeDefaultValue()]
    [PageSizeRange]
    [FromQuery(Name = "pageSize")]
    [Description("The pagesize of the result.")]
    public int PageSize { get; set; }

    [FromQuery(Name = "sort")]
    [GenericMaxLength]
    [GenericRegularExpression]
    [Description(@"The sorting query expression can be built with a comma-delimited sorted list of field/property names, followed by `asc` or `desc` keywords. 

By default, if you don't add these keywords, the API assumes you need Ascending sorting.")]
    public string? OrderBy { get; set; }

    [FromQuery(Name = "filter")]
    [GenericMaxLength]
    [GenericRegularExpression]
    [Description(@"The following filter operators are supported:

### Conditional Operators

| Name                  | Operator | Usage example        |
|-----------------------|----------|----------------------|
| Equal                 | `=`      | `FieldName = Value`  |
| NotEqual              | `!=`     | `FieldName !=Value`  |
| LessThan              | `<`      | `FieldName < Value`  |
| GreaterThan           | `>`      | `FieldName > Value`  |
| GreaterThanOrEqual    | `>=`     | `FieldName >=Value`  |
| LessThanOrEqual       | `<=`     | `FieldName <=Value`  |
| Contains - Like       | `=*`     | `FieldName =*Value`  |
| NotContains - NotLike | `!*`     | `FieldName !*Value`  |
| StartsWith            | `^`      | `FieldName ^ Value`  |
| NotStartsWith         | `!^`     | `FieldName !^ Value` |
| EndsWith              | `$`      | `FieldName $ Value`  |
| NotEndsWith           | `!$`     | `FieldName !$ Value` |

> Tip: If you don't specify any value after `=` or `!=` operators, the API searches for the `default` and `null` values.

### Logical Operators

| Name        | Operator | Usage example                                   |
|-------------|----------|-------------------------------------------------|
| AND         | `,`      | `FirstName = Value, LastName = Value2`          |
| OR          | `\|`     | `FirstName=Value\|LastName=Value2`              |
| Parenthesis | `()`     | `(FirstName=*Jo,Age<30)\|(FirstName!=Hn,Age>30)`|

### Case Insensitive Operator

The `/i` operator can be use after string values for case insensitive searches. You should only use this operator after the search value. 

Example:
```
FirstName=John/i
```

this query matches with `JOHN`, `john`, `John`, `jOHn`, etc.")]
    public string? Filter { get; set; }
}