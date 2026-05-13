using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Cleaning.Application;
using Cleaning.Application.Options;
using Cleaning.Core.Contracts;
using Cleaning.Core.DTOs;
using Cleaning.Core.Enums;
using Cleaning.Infra;
using Cleaning.Infra.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<MaintenanceAlertOptions>(builder.Configuration.GetSection(MaintenanceAlertOptions.SectionName));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (allowedOrigins is { Length: > 0 })
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
            return;
        }

        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

await EnsureDatabaseAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

app.UseCors("frontend");
app.UseWhen(
    context => !IsHealthCheckRequest(context.Request.Path),
    branch => branch.UseHttpsRedirection());

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new
{
    message = "Cleaning.API online",
    module = "maintenance-control"
}));

var tasks = app.MapGroup("/api/tasks").WithTags("Tasks");

tasks.MapGet("/", async (string? filter, IMaintenanceTaskService service, CancellationToken cancellationToken) =>
{
    if (!TryParseFilter(filter, out var parsedFilter))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["filter"] = ["Use all, pending ou overdue."]
        });
    }

    var result = await service.ListAsync(parsedFilter, cancellationToken);
    return Results.Ok(result);
});

tasks.MapPost("/", async (CreateMaintenanceTaskRequest request, IMaintenanceTaskService service, CancellationToken cancellationToken) =>
{
    var validationErrors = Validate(request);
    if (validationErrors is not null)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var result = await service.CreateAsync(request, cancellationToken);
    return Results.Created($"/api/tasks/{result.Id}", result);
});

tasks.MapPost("/{id:guid}/complete", async (Guid id, IMaintenanceTaskService service, CancellationToken cancellationToken) =>
{
    var result = await service.CompleteAsync(id, cancellationToken);
    return result is null
        ? Results.NotFound()
        : Results.Ok(result);
});

tasks.MapDelete("/{id:guid}", async (Guid id, IMaintenanceTaskService service, CancellationToken cancellationToken) =>
{
    var deleted = await service.DeleteAsync(id, cancellationToken);
    return deleted
        ? Results.NoContent()
        : Results.NotFound();
});

app.MapGet("/api/alerts", async (IMaintenanceAlertService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.GetAlertsAsync(cancellationToken)))
    .WithTags("Maintenance");

app.MapGet("/api/analysis", async (IMaintenanceAnalysisService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.AnalyzeAsync(cancellationToken)))
    .WithTags("Maintenance");

app.MapDefaultEndpoints();

app.Run();

static async Task EnsureDatabaseAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CleaningDbContext>();
    await dbContext.Database.MigrateAsync();
}

static bool TryParseFilter(string? filter, out MaintenanceTaskFilter parsedFilter)
{
    if (string.IsNullOrWhiteSpace(filter))
    {
        parsedFilter = MaintenanceTaskFilter.All;
        return true;
    }

    return Enum.TryParse(filter, true, out parsedFilter);
}

static Dictionary<string, string[]>? Validate<T>(T model)
{
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(model!);

    if (Validator.TryValidateObject(model!, validationContext, validationResults, true))
    {
        return null;
    }

    return validationResults
        .GroupBy(
            result => result.MemberNames.FirstOrDefault() ?? string.Empty,
            result => result.ErrorMessage ?? "Valor invalido.")
        .ToDictionary(group => group.Key, group => group.ToArray());
}

static bool IsHealthCheckRequest(PathString path)
    => path.StartsWithSegments("/health") || path.StartsWithSegments("/alive");
