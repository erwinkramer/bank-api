var builder = DistributedApplication.CreateBuilder(args);

// should be replaced in the future when following is implemented: https://github.com/microsoft/aspire/issues/10743
var env = File.ReadLines(Path.Combine(builder.Environment.ContentRootPath, "..", ".env"))
            .Select(line => line.Split('=', 2))
            .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);

var s3Proxy = builder.AddDockerfile("S3Proxy", "../Sidecar.S3Proxy").WithHttpEndpoint(port: 6070, targetPort: 6070);
foreach (var entry in env)
    s3Proxy.WithEnvironment(entry.Key, entry.Value);

var dapr = builder.AddDockerfile("Dapr", "../Sidecar.Dapr").WithEndpoint(port: 50002, targetPort: 50002);
foreach (var entry in env)
    dapr.WithEnvironment(entry.Key, entry.Value);

builder.AddProject<Projects.BankApi_Service_Stable>("BankApiService-Stable")
    .WaitFor(s3Proxy)
    .WaitFor(dapr);

builder.AddProject<Projects.BankApi_Service_Beta>("BankApiService-Beta")
    .WaitFor(s3Proxy)
    .WaitFor(dapr);

builder.AddProject<Projects.BankApi_Mcp>("BankApi-Mcp");

builder.Build().Run();
