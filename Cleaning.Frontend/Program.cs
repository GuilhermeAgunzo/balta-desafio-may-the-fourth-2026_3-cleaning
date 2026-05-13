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

var apiBaseUrl = string.IsNullOrWhiteSpace(frontendOptions.ApiBaseUrl)
    ? builder.HostEnvironment.BaseAddress
    : frontendOptions.ApiBaseUrl;

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<CleaningApiClient>();

await builder.Build().RunAsync();
