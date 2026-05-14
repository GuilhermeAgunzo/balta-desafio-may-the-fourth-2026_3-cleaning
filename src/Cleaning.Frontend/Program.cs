using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Cleaning.Frontend;
using Cleaning.Frontend.Options;
using Cleaning.Frontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var frontendOptions = new FrontendOptions();
builder.Configuration.GetSection(FrontendOptions.SectionName).Bind(frontendOptions);

var apiBaseUrl = ResolveApiBaseUrl(builder.Configuration, frontendOptions, builder.HostEnvironment.BaseAddress);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<CleaningApiClient>();

await builder.Build().RunAsync();

static string ResolveApiBaseUrl(IConfiguration configuration, FrontendOptions frontendOptions, string hostBaseAddress)
    => new[]
    {
        configuration["services:api:https:0"],
        configuration["services:api:http:0"],
        configuration["API_HTTPS"],
        configuration["API_HTTP"],
        frontendOptions.ApiBaseUrl,
        hostBaseAddress
    }.FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value))
    ?? throw new InvalidOperationException("Nao foi possivel determinar a URL base da API.");
