#pragma warning disable ASPIRECOMPUTE003

var builder = DistributedApplication.CreateBuilder(args);

var registry = builder.AddContainerRegistry("ghcr", "ghcr.io:443", "erwinkramer");
var k8s = builder.AddKubernetesEnvironment("k8s").WithContainerRegistry(registry);

var dotEnvParams = builder.AddParametersFromDotEnv();

var postgresUsername = builder.AddParameter("bank-api-db-username", "admin");
var postgresPassword = builder.AddParameter("bank-api-db-password", "admin", secret: true);
var postgres = builder.AddPostgres("bank-api-db", postgresUsername, postgresPassword, 5432)
    .WithImageTag("18-alpine");

var s3Proxy = builder.AddDockerfile("bank-api-s3proxy", "../Sidecar.S3Proxy")
    .WithHttpEndpoint(port: 6070, targetPort: 6070)
    .WithHttpHealthCheck("/healthz")
    .WithEnvironmentParameters(dotEnvParams);

var dapr = builder.AddDockerfile("bank-api-dapr", "../Sidecar.Dapr")
    .WithHttpEndpoint(port: 3500, targetPort: 3500, name: "dapr-http")
    .WithEndpoint(port: 50002, targetPort: 50002, name: "dapr-grpc")
    .WithHttpHealthCheck("/v1.0/healthz", endpointName: "dapr-http", statusCode: 204)
    .WithEnvironmentParameters(dotEnvParams);

var apiStable = builder.AddProject<Projects.BankApi_Service_Stable>("bank-api")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/.well-known/jwks.json")
    .WaitFor(postgres).WithReference(postgres)
    .WaitFor(s3Proxy)
    .WaitFor(dapr);

var apiBeta = builder.AddProject<Projects.BankApi_Service_Beta>("bank-api-beta")
    .WithExplicitStart()
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/.well-known/jwks.json")
    .WaitFor(postgres).WithReference(postgres)
    .WaitFor(s3Proxy)
    .WaitFor(dapr);

var mcpStable = builder.AddProject<Projects.BankApi_Mcp>("bank-api-mcp")
    .WithExternalHttpEndpoints();

k8s.AddGateway("bank-gateway")
    .WithGatewayClass("bank-gateway-class")
    .WithGatewayAnnotation("metallb.io/loadBalancerIPs", "192.168.0.30")
    .WithHostname("guanchen.nl").WithTls()
    .WithRoute("/v1", apiStable.GetEndpoint("http"))
    .WithRoute("/v2", apiBeta.GetEndpoint("http"))
    .WithRoute("/mcp", mcpStable.GetEndpoint("http"));

builder.Build().Run();
