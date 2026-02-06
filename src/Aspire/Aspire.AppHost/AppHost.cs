var builder = DistributedApplication.CreateBuilder(args);

//var esPassword = builder.AddParameter("es-password", secret: true);
//var elasticSearch = builder.AddElasticsearch("elasticsearch", esPassword);

var elasticSearch = builder.AddElasticsearch("elasticsearch")
//.WithDataVolume(isReadOnly: false)
//.WithLifetime(ContainerLifetime.Persistent)
.WithContainerRuntimeArgs("--memory=512m");

var rabbitMq = builder.AddRabbitMQ("rabbitmq");

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
var shoppingCartDb = postgres.AddDatabase("shoppingcartdb");
var ordersDb = postgres.AddDatabase("ordersdb");
var paymentDb = postgres.AddDatabase("paymentdb");
var shippingDb = postgres.AddDatabase("shippingdb");
var infrasDb = postgres.AddDatabase("infrasdb");

var eshopApi = builder.AddProject<Projects.API>("eshop-api")
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
    .WithReference(shoppingCartDb)
        .WaitFor(shoppingCartDb)
    .WithReference(ordersDb)
        .WaitFor(ordersDb)
    .WithReference(paymentDb)
        .WaitFor(paymentDb)
    .WithReference(shippingDb)
        .WaitFor(shippingDb)
    .WithReference(infrasDb)
        .WaitFor(infrasDb)
    .WithReference(elasticSearch)
        .WaitFor(elasticSearch)
    .WithReference(rabbitMq)
        .WaitFor(rabbitMq);

builder.AddProject<Projects.BlazorAdmin>("eshop-admin")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(eshopApi)
    .WaitFor(eshopApi);

builder.AddDockerfile("eshop-reactapp", "../../Apps/react-app")
    .WithHttpEndpoint(port: 3002, targetPort: 80)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(eshopApi)
    .WaitFor(eshopApi);


builder.Build().Run();
