var builder = DistributedApplication.CreateBuilder(args);

var blobs = builder.AddAzureStorage("storage")
                   .RunAsEmulator()
                   .AddBlobs("BankStorage");

builder.AddProject<Projects.bank_api_minimal>("apiservice")
       .WithReference(blobs);


builder.Build().Run();
