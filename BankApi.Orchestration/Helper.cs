public static class Helper
{
    /// <summary>
    /// Reads a .env file and adds its contents as parameters to the builder as secrets
    /// Should be replaced in the future when following is implemented: https://github.com/microsoft/aspire/issues/10743
    /// </summary>
    public static List<IResourceBuilder<ParameterResource>> AddParametersFromDotEnv(this IDistributedApplicationBuilder builder)
    {
        var parameters = new List<IResourceBuilder<ParameterResource>>();

        var env = File.ReadLines(Path.Combine(builder.Environment.ContentRootPath, "..", ".env"))
            .Select(line => line.Split('=', 2))
            .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);

        foreach (var entry in env)
        {
            // Name must contain only ASCII letters, digits, and hyphens, 
            // so we're sanitizing the keys by replacing underscores with hyphens.
            parameters.Add(builder.AddParameter(entry.Key.Replace('_', '-'), entry.Value, secret: true));
        }

        return parameters;
    }

    /// <summary>
    /// Injects a collection of parameters into a resource as environment variables, reversing the hyphen-to-underscore naming
    /// </summary>
    public static IResourceBuilder<T> WithEnvironmentParameters<T>(this IResourceBuilder<T> builder, IEnumerable<IResourceBuilder<ParameterResource>> parameters)
        where T : IResourceWithEnvironment
    {
        foreach (var entry in parameters)
        {
            builder.WithEnvironment(entry.Resource.Name.Replace('-', '_'), entry.Resource);
        }

        return builder;
    }
}