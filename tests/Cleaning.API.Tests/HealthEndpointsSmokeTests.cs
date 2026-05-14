using System.Net;

namespace Cleaning.API.Tests;

public sealed class HealthEndpointsSmokeTests
{
    [Fact]
    public async Task Root_ShouldRedirectToHttps_InDevelopment()
    {
        using var factory = new ApiApplicationFactory();
        using var client = factory.CreateApiClient(useHttps: false);

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
        Assert.Equal("https://localhost:5071/", response.Headers.Location?.ToString());
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/alive")]
    public async Task HealthEndpoints_ShouldReturnOkWithoutHttpsRedirect_InDevelopment(string path)
    {
        using var factory = new ApiApplicationFactory();
        using var client = factory.CreateApiClient(useHttps: false);

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }
}
