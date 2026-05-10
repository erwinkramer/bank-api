var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BankApi_Service_Stable>("BankApiService-Stable");

builder.AddProject<Projects.BankApi_Service_Beta>("BankApiService-Beta");

builder.AddProject<Projects.BankApi_Mcp>("BankApi-Mcp");

builder.Build().Run();
