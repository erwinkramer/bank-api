using Microsoft.OpenApi;

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
