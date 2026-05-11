var builder = DistributedApplication.CreateBuilder(args);

var sqlPam = builder.AddSqlServer("pam-sql");
var dbPam = sqlPam.AddDatabase("pam-db");

var sqlFpcc = builder.AddSqlServer("fpcc-sql");
var dbFpcc = sqlFpcc.AddDatabase("fpcc-db");

var serviceBus = builder.AddAzureServiceBus("servicebus")
    .RunAsEmulator();
var queue = serviceBus.AddServiceBusQueue("withdrawal-incoming");

var pam = builder.AddProject<Projects.PAM>("PAM")
    .WithReference(dbPam)
    .WaitFor(dbPam)
    .WithReference(serviceBus)
    .WaitFor(serviceBus);

var fpcc = builder.AddProject<Projects.FPCC>("FPCC")
    .WithReference(dbFpcc)
    .WaitFor(dbFpcc)
    .WithReference(serviceBus)
    .WaitFor(serviceBus);

builder.Build().Run();