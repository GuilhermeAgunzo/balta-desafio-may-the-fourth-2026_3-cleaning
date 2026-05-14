using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cleaning.API.Tests;

internal sealed class ApiApplicationFactory : WebApplicationFactory<global::Program>
{
    private const int HttpsPort = 5071;
    private readonly string _contentRoot = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        "..",
        "src",
        "Cleaning.API"));
    private readonly string _databasePath = Path.Combine(
        AppContext.BaseDirectory,
        $"cleaning-api-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(_contentRoot);
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("ConnectionStrings:CleaningDb", $"Data Source={_databasePath}"),
                new KeyValuePair<string, string?>("OpenAI:ApiKey", string.Empty)
            ]);
        });
        builder.ConfigureServices(services =>
            services.Configure<HttpsRedirectionOptions>(options => options.HttpsPort = HttpsPort));
    }

    public HttpClient CreateApiClient(bool useHttps = true)
        => CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri(useHttps ? "https://localhost" : "http://localhost")
        });

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
