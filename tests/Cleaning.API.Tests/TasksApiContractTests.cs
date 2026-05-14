using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Cleaning.API.Tests;

public sealed class TasksApiContractTests
{
    [Fact]
    public async Task GetTasks_WithInvalidFilter_ShouldReturnValidationProblem()
    {
        using var factory = new ApiApplicationFactory();
        using var client = factory.CreateApiClient();

        var response = await client.GetAsync("/api/tasks?filter=desconhecido");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var error = document.RootElement
            .GetProperty("errors")
            .GetProperty("filter")[0]
            .GetString();

        Assert.Equal("Use all, pending ou overdue.", error);
    }

    [Fact]
    public async Task PostThenGetTasks_ShouldUseStringEnumsExpectedByFrontend()
    {
        using var factory = new ApiApplicationFactory();
        using var client = factory.CreateApiClient();

        var createRequest = new
        {
            name = "Limpar droides",
            description = "Remover poeira de todos os compartimentos dos droides de servico.",
            recurrenceInterval = 2,
            recurrenceUnit = "Weeks",
            firstExecutionDate = "2026-05-20"
        };

        var createResponse = await client.PostAsJsonAsync("/api/tasks", createRequest);
        var listResponse = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        using var createdDocument = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var createdTask = createdDocument.RootElement;

        Assert.Equal("Weeks", createdTask.GetProperty("recurrenceUnit").GetString());
        Assert.Equal("Pending", createdTask.GetProperty("status").GetString());

        using var listedDocument = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        var listedTask = listedDocument.RootElement
            .EnumerateArray()
            .Single(task => task.GetProperty("id").GetString() == createdTask.GetProperty("id").GetString());

        Assert.Equal(createdTask.GetProperty("id").GetString(), listedTask.GetProperty("id").GetString());
        Assert.Equal("Weeks", listedTask.GetProperty("recurrenceUnit").GetString());
        Assert.Equal("Pending", listedTask.GetProperty("status").GetString());
    }
}
