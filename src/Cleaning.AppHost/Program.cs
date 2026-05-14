var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject("api", @"..\Cleaning.API\Cleaning.API.csproj", launchProfileName: "https")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.AddProject("frontend", @"..\Cleaning.Frontend\Cleaning.Frontend.csproj", launchProfileName: "https")
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
