var builder = DistributedApplication.CreateBuilder(args);

var rabbitMq = builder.AddRabbitMQ("rabbitmq");

//var esPassword = builder.AddParameter("es-password", secret: true);
//var elasticSearch = builder.AddElasticsearch("elasticsearch", esPassword);

//var elasticSearch = builder.AddElasticsearch("elasticsearch")
//.WithDataVolume(isReadOnly: false)
//.WithLifetime(ContainerLifetime.Persistent)
//.WithContainerRuntimeArgs("--memory=512m");

//var pgUsername = builder.AddParameter("db-username", secret: true);
//var pgPassword = builder.AddParameter("db-password", secret: true);
//var postgres = builder.AddPostgres("postgres-eshop", pgUsername, pgPassword)

var postgres = builder.AddPostgres("postgres-eshop")
    //.WithDataVolume(isReadOnly: false)
    .WithPgWeb(pgAdmin => pgAdmin.WithHostPort(5050));

var authDb = postgres.AddDatabase("authdb");
var usersDb = postgres.AddDatabase("usersdb");
var catalogDb = postgres.AddDatabase("catalogdb");
var inventoryDb = postgres.AddDatabase("inventorydb");
var pricingDb = postgres.AddDatabase("pricingdb");

var api = builder.AddProject<Projects.API>("api")
    .WithHttpHealthCheck("/health")
    .WithReference(catalogDb)
        .WaitFor(catalogDb)
    .WithReference(inventoryDb)
        .WaitFor(inventoryDb)
    .WithReference(pricingDb)
        .WaitFor(pricingDb)
    .WithReference(authDb)
        .WaitFor(authDb)
    .WithReference(usersDb)
        .WaitFor(usersDb)
    //.WithReference(elasticSearch)
    //    .WaitFor(elasticSearch)
    .WithReference(rabbitMq)
        .WaitFor(rabbitMq);

//builder.AddProject<Projects.BlazorAdmin>("webadmin")
//    .WithExternalHttpEndpoints()
//    .WithHttpHealthCheck("/health")
//    .WithReference(api)
//    .WaitFor(api);

builder.Build().Run();
