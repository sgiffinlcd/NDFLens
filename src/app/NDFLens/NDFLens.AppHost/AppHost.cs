var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", secret: true);

var sql = builder.AddSqlServer("SQLNDFLens", password: sqlPassword, port: 53000)
    .WithContainerName("SQLNDFLens")
    .WithDataVolume("SQLNDFLens-data")
    .WithLifetime(ContainerLifetime.Persistent);

var db = sql.AddDatabase("NDFLogs");


builder.AddProject<Projects.NDFLens_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WaitFor(db);
builder.Build().Run();
