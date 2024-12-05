var builder = DistributedApplication.CreateBuilder(args);

var blobs = builder.AddAzureStorage("storage")
                   .RunAsEmulator()
                   .AddBlobs("BankStorage");

builder.AddProject<Projects.BankApi_Service_V1>("BankApiService-V1")
       .WithReference(blobs);

builder.AddProject<Projects.BankApi_Service_V2>("BankApiService-V2")
       .WithReference(blobs);

builder.Build().Run();
