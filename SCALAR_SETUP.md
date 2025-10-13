# Scalar API Documentation Setup

## Overview

Scalar has been successfully integrated into the Elsa.Aspire.Server project to provide interactive API documentation and testing capabilities.

## What Was Added

### NuGet Packages
- `Scalar.AspNetCore` (v2.8.11) - Main Scalar package for rendering the API documentation UI
- `Microsoft.AspNetCore.OpenApi` (v9.0.9) - OpenAPI specification generation for ASP.NET Core

### Configuration Changes

#### Program.cs Updates

1. **OpenAPI Configuration** (Lines 106-119):
   ```csharp
   builder.Services.AddOpenApi(options =>
   {
       options.AddDocumentTransformer((document, context, cancellationToken) =>
       {
           document.Info = new()
           {
               Title = "Elsa Workflows API",
               Version = "v1",
               Description = "API for managing and executing Elsa workflows..."
           };
           return Task.CompletedTask;
       });
   });
   ```

2. **Scalar Middleware Configuration** (Lines 131-141):
   ```csharp
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
   ```

3. **Required Using Statement**:
   ```csharp
   using Scalar.AspNetCore;
   ```

## How to Access Scalar

### Running with Aspire (Recommended)

1. Start the Aspire application:
   ```bash
   dotnet run --project Elsa.Aspire.AppHost/Elsa.Aspire.AppHost.csproj
   ```

2. Open the Aspire Dashboard:
   - The dashboard URL will be displayed in the console output
   - Example: `https://localhost:17296/login?t=...`

3. Find the elsaserver service ports:
   - In the Aspire dashboard, look for the "elsaserver" resources
   - Note the HTTP endpoint URLs for each replica

4. Access Scalar:
   - Navigate to: `http://localhost:<server-port>/scalar`
   - Example: `http://localhost:5062/scalar`

### Endpoints Available in Scalar

The Scalar UI will document all controller endpoints including:

- `GET /api/workflow/definitions` - Get available workflow information
- `POST /api/workflow/trigger` - Manually trigger a workflow
- `GET /api/workflow/status` - Get status of running workflows
- `GET /api/workflow/test` - Test workflow execution endpoint

Plus all Elsa framework API endpoints from:
- Workflow Management APIs
- Workflow Runtime APIs
- SignalR Hubs

## Features

- **Interactive API Testing**: Test all endpoints directly from the browser
- **Code Generation**: Generate client code in various languages (C#, Python, JavaScript, etc.)
- **OpenAPI Specification**: Full OpenAPI 3.0 specification at `/openapi/v1.json`
- **Beautiful UI**: Modern, responsive interface with purple theme
- **Authentication Support**: Supports the Keycloak JWT bearer authentication configured in the project

## Troubleshooting

### Can't Find Scalar Endpoint

If you can't access `/scalar`:
1. Ensure the server is running
2. Check the server logs for the listening ports
3. Verify the URL includes the correct port number
4. Try accessing the OpenAPI spec directly at `/openapi/v1.json`

### Server Won't Start

The server requires:
- PostgreSQL database (provided by Aspire)
- RabbitMQ messaging (provided by Aspire)
- Keycloak authentication (provided by Aspire)

Always run through the Aspire AppHost for proper dependency management.

## Technical Details

- **Framework**: ASP.NET Core (.NET 10)
- **API Documentation**: OpenAPI 3.0
- **UI Framework**: Scalar v2.8.11
- **Theme**: Purple
- **Default Language**: C# with HttpClient
