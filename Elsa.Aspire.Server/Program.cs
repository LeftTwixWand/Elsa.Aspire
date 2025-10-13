using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.Extensions;
using Medallion.Threading.Postgres;
using Microsoft.AspNetCore.Authorization;
using Elsa.Workflows.Runtime.Distributed.Extensions;
using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddAuthentication()
                .AddKeycloakJwtBearer(
                    serviceName: "keycloak",
                    realm: "Elsa",
                    options =>
                    {
                        options.Audience = "account";
                        options.RequireHttpsMetadata = false;
                    });

builder.Services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>();

builder.Services.AddElsa(elsa =>
{
    // Configure Management layer to use EF Core.
    elsa.UseWorkflowManagement(management =>
        management.UseEntityFrameworkCore( ef =>
        ef.UsePostgreSql(builder.Configuration.GetConnectionString("elsadb")!)));

    // Configure Runtime layer to use EF Core.
    elsa.UseWorkflowRuntime(runtime =>
    {
        runtime.UseEntityFrameworkCore(ef =>
            ef.UsePostgreSql(builder.Configuration.GetConnectionString("elsadb")!));

        runtime.UseDistributedRuntime();

        runtime.DistributedLockProvider = _ =>
            new PostgresDistributedSynchronizationProvider(builder.Configuration.GetConnectionString("elsadb")!,
                options =>
                {
                    options.KeepaliveCadence(TimeSpan.FromMinutes(5));
                    options.UseMultiplexing();
                });
    });

    elsa.UseDistributedCache(distributedCaching =>
    {
        distributedCaching.UseMassTransit();
    });

    // Expose Elsa API endpoints.
    elsa.UseWorkflowsApi();

    // Setup a SignalR hub for real-time updates from the server.
    elsa.UseRealTimeWorkflows();

    elsa.UseJavaScript();

    // Enable HTTP activities.
    elsa.UseHttp(http =>
    {
        http.ConfigureHttpOptions = options => builder.Configuration.GetSection("Http").Bind(options);
    });

    // Use timer activities.
    elsa.UseScheduling(scheduling => scheduling.UseQuartzScheduler());

    elsa.UseQuartz(quartz => quartz.UsePostgreSql(builder.Configuration.GetConnectionString("elsadb")!));

    elsa.UseMassTransit(masstransit =>
    {
        masstransit.UseRabbitMq(builder.Configuration.GetConnectionString("messaging")!,
            rabbitMqFeature => rabbitMqFeature.ConfigureTransportBus = (context, bus) =>
            {
                bus.PrefetchCount = 4;
                bus.Durable = true;
                bus.AutoDelete = false;
                bus.ConcurrentMessageLimit = 32;
                // etc.
            }
        );
    });

    // Register custom activities from the application, if any.
    elsa.AddActivitiesFrom<Program>();

    // Register custom workflows from the application, if any.
    elsa.AddWorkflowsFrom<Program>();
});

// Configure CORS to allow designer app hosted on a different origin to invoke the APIs.
builder.Services.AddCors(cors => cors
    .AddDefaultPolicy(policy => policy
        .AllowAnyOrigin() // For demo purposes only. Use a specific origin instead.
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders("x-elsa-workflow-instance-id"))); // Required for Elsa Studio in order to support running workflows from the designer. Alternatively, you can use the `*` wildcard to expose all headers.

// Add Controllers for custom API endpoints.
builder.Services.AddControllers();

// Configure OpenAPI
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "Elsa Workflows API",
            Version = "v1",
            Description = "API for managing and executing Elsa workflows with comprehensive documentation for testing workflows, managing definitions, and monitoring execution status."
        };
        return Task.CompletedTask;
    });
});

// Add Health Checks.
builder.Services.AddHealthChecks();

var app = builder.Build();


// Configure web application's middleware pipeline.
app.UseCors();
app.MapHealthChecks("/health");

// Map OpenAPI endpoint
app.MapOpenApi();

// Configure Scalar API documentation UI at /scalar
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("Elsa Workflows API")
        .WithTheme(ScalarTheme.Purple)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseRouting(); // Required for SignalR.
app.UseAuthentication();
app.UseAuthorization();
app.UseWorkflowsApi(); // Use Elsa API endpoints.
app.UseWorkflows(); // Use Elsa middleware to handle HTTP requests mapped to HTTP Endpoint activities.
app.UseWorkflowsSignalRHubs(); // Optional SignalR integration. Elsa Studio uses SignalR to receive real-time updates from the server.
app.MapControllers(); // Map controller endpoints.


app.Run();
