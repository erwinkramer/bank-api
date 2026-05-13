#pragma warning disable ASPIRECOMPUTE003

var builder = DistributedApplication.CreateBuilder(args);

var registry = builder.AddContainerRegistry("ghcr", "ghcr.io:443", "erwinkramer");
var k8s = builder.AddKubernetesEnvironment("k8s").WithContainerRegistry(registry);

var dotEnvParams = builder.AddParametersFromDotEnv();

var s3Proxy = builder.AddDockerfile("bank-api-s3proxy", "../Sidecar.S3Proxy").WithHttpEndpoint(port: 6070, targetPort: 6070);
s3Proxy.WithEnvironmentParameters(dotEnvParams);

var dapr = builder.AddDockerfile("bank-api-dapr", "../Sidecar.Dapr").WithEndpoint(port: 50002, targetPort: 50002);
dapr.WithEnvironmentParameters(dotEnvParams);

var apiStable = builder.AddProject<Projects.BankApi_Service_Stable>("bank-api")
    .WaitFor(s3Proxy)
    .WaitFor(dapr);

var apiBeta = builder.AddProject<Projects.BankApi_Service_Beta>("bank-api-beta")
    .WaitFor(s3Proxy)
    .WaitFor(dapr);

var mcpStable = builder.AddProject<Projects.BankApi_Mcp>("bank-api-mcp")
    .WaitFor(s3Proxy)
    .WaitFor(dapr);

k8s.AddGateway("bank-gateway")
    .WithGatewayClass("bank-gateway-class")
    .WithGatewayAnnotation("metallb.io/loadBalancerIPs", "192.168.6.7")
    .WithHostname("guanchen.nl").WithTls()
    .WithRoute("/v1", apiStable.GetEndpoint("http"))
    .WithRoute("/v2", apiBeta.GetEndpoint("http"))
    .WithRoute("/mcp", mcpStable.GetEndpoint("http"));

builder.Build().Run();
