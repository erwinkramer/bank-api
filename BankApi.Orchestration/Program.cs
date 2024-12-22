using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var blobs = builder.AddAzureStorage("storage")
                   .RunAsEmulator()
                   .AddBlobs("BankStorage");

builder.AddProject<Projects.BankApi_Service_Stable>("BankApiService-Stable")
       .WithReference(blobs);

builder.AddProject<Projects.BankApi_Service_Beta>("BankApiService-Beta")
       .WithReference(blobs);

builder.Build().Run();
