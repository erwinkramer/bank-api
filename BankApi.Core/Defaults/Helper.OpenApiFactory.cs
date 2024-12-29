/*

Source is mostly: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/src/Swashbuckle.AspNetCore.SwaggerGen/SwaggerGenerator/OpenApiAnyFactory.cs

*/

using System.Text.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

public class OpenApiFactory
{
    public static OpenApiHeader CreateHeaderInt(string? description = null) => new ()
    {
        Description = description,
        Schema = CreateSchemaRef("GenericInt")
    };

    public static OpenApiHeader CreateHeaderString(string? description = null) => new ()
    {
        Description = description,
        Schema = CreateSchemaRef("GenericString")
    };

    public static OpenApiHeader CreateHeaderRef(string headerId, string? description = null) => new ()
    {
        Description = description,
        Reference = new ()
        {
            Type = ReferenceType.Header,
            Id = headerId
        }
    };

    public static OpenApiSchema CreateSchemaRef(string schemaId) => new ()
    {
        Reference = new ()
        {
            Type = ReferenceType.Schema,
            Id = schemaId
        }
    };

    public static OpenApiSecurityScheme CreateSecuritySchemaRef(string schemaId) => new ()
    {
        Reference = new ()
        {
            Type = ReferenceType.SecurityScheme,
            Id = schemaId
        }
    };

    public static IOpenApiAny? CreateFromJson(string json)
    {
        try
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            return CreateFromJsonElement(jsonElement);
        }
        catch { }

        return null;
    }

    static IOpenApiAny CreateOpenApiArray(JsonElement jsonElement)
    {
        OpenApiArray openApiArray = [];

        foreach (var item in jsonElement.EnumerateArray())
        {
            openApiArray.Add(CreateFromJsonElement(item));
        }

        return openApiArray;
    }

    static IOpenApiAny CreateOpenApiObject(JsonElement jsonElement)
    {
        OpenApiObject openApiObject = [];

        foreach (var property in jsonElement.EnumerateObject())
        {
            openApiObject.Add(property.Name, CreateFromJsonElement(property.Value));
        }

        return openApiObject;
    }

    static IOpenApiAny CreateFromJsonElement(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind == JsonValueKind.Null)
            return new OpenApiNull();

        if (jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
            return new OpenApiBoolean(jsonElement.GetBoolean());

        if (jsonElement.ValueKind == JsonValueKind.Number)
        {
            if (jsonElement.TryGetInt32(out int intValue))
                return new OpenApiInteger(intValue);

            if (jsonElement.TryGetInt64(out long longValue))
                return new OpenApiLong(longValue);

            if (jsonElement.TryGetSingle(out float floatValue) && !float.IsInfinity(floatValue))
                return new OpenApiFloat(floatValue);

            if (jsonElement.TryGetDouble(out double doubleValue))
                return new OpenApiDouble(doubleValue);
        }

        if (jsonElement.ValueKind == JsonValueKind.String)
            return new OpenApiString(jsonElement.ToString());

        if (jsonElement.ValueKind == JsonValueKind.Array)
            return CreateOpenApiArray(jsonElement);

        if (jsonElement.ValueKind == JsonValueKind.Object)
            return CreateOpenApiObject(jsonElement);

        throw new System.ArgumentException($"Unsupported value kind {jsonElement.ValueKind}");
    }
}
