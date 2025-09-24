using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// External dependencies removed - using in-memory storage
// var postgres = builder.AddPostgres("postgres")
//          .WithDataVolume(isReadOnly: false)
//          .WithLifetime(ContainerLifetime.Persistent);
//
// var postgresdb = postgres.AddDatabase("elsadb");
//
// var messaging = builder.AddRabbitMQ("messaging")
//         .WithLifetime(ContainerLifetime.Persistent);
//
// var keycloak = builder.AddKeycloak("keycloak", 8080)
//         .WithDataVolume()
//         .WithRealmImport("./Realms")
//         .WithLifetime(ContainerLifetime.Persistent);

var server = builder.AddProject<Projects.Elsa_Aspire_Server>("elsaserver")
        .WithReplicas(1)
        .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Elsa_Aspire_Studio>("elsastudio")
    .WithReference(server)
    .WaitFor(server);

builder.Build().Run();
