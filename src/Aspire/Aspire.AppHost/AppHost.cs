var builder = DistributedApplication.CreateBuilder(args);

//var esPassword = builder.AddParameter("es-password", secret: true);
//var elasticSearch = builder.AddElasticsearch("elasticsearch", esPassword);

var elasticSearch = builder.AddElasticsearch("elasticsearch");
    //.WithDataVolume(isReadOnly: false);

var pgUsername = builder.AddParameter("eshop-username", secret: true);
var pgPassword = builder.AddParameter("eshop-password", secret: true);

var postgres = builder.AddPostgres("postgres-eshop", pgUsername, pgPassword)
    //.WithDataVolume(isReadOnly: false)
    .WithPgWeb(pgAdmin => pgAdmin.WithHostPort(5050));

var catalogDb = postgres.AddDatabase("catalogdb");

var api = builder.AddProject<Projects.API>("api")
    .WithHttpHealthCheck("/health")
    .WithReference(catalogDb)
        .WaitFor(catalogDb)
    .WithReference(elasticSearch)
        .WaitFor(elasticSearch);

//builder.AddProject<Projects.BlazorAdmin>("webadmin")
//    .WithExternalHttpEndpoints()
//    .WithHttpHealthCheck("/health")
//    .WithReference(api)
//    .WaitFor(api);

builder.Build().Run();
