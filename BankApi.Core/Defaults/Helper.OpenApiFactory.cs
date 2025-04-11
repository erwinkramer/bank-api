using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;

public class OpenApiFactory
{
    public static OpenApiHeader CreateHeaderInt(OpenApiDocument doc, string? description = null) => new()
    {
        Description = description,
        Schema = new OpenApiSchemaReference("GenericInt", doc)
    };

    public static OpenApiHeader CreateHeaderString(OpenApiDocument doc, string? description = null) => new()
    {
        Description = description,
        Schema = new OpenApiSchemaReference("GenericString", doc)
    };
}
