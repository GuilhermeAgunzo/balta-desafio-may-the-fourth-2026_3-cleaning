var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject("api", @"..\Cleaning.API\Cleaning.API.csproj")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.AddProject("frontend", @"..\Cleaning.Frontend\Cleaning.Frontend.csproj")
    .WithExternalHttpEndpoints()
    .WaitFor(api);

builder.Build().Run();
