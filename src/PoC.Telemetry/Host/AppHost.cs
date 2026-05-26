using System.Net;
using System.Net.Sockets;

var builder = DistributedApplication.CreateBuilder(args);

// Podman on Windows: host.containers.internal resolves to Podman's own bridge gateway (10.89.x.1),
// not the Windows host. We must pass the actual host LAN/VPN IP so the collector can reach
// the Aspire Dashboard which runs on the host (via Rider).
static string GetHostIp()
{
    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    socket.Connect("8.8.8.8", 80);
    return ((IPEndPoint)socket.LocalEndPoint!).Address.ToString();
}
var hostIp = GetHostIp();

var sqlPam = builder.AddSqlServer("pam-sql");
var dbPam = sqlPam.AddDatabase("pam-db");

var sqlFpcc = builder.AddSqlServer("fpcc-sql");
var dbFpcc = sqlFpcc.AddDatabase("fpcc-db");

var serviceBus = builder.AddAzureServiceBus("servicebus")
    .RunAsEmulator();
var queue = serviceBus.AddServiceBusQueue("withdrawal-incoming");
var collector = builder.AddContainer("otel-collector", "otel/opentelemetry-collector-contrib", "0.152.0")
    .WithBindMount("./CollectorConfig/collector-config.yaml", "/etc/otelcol-contrib/config.yaml")
    .WithEndpoint(port: 4317, targetPort: 4317, name: "grpc", scheme: "http", isProxied: false)
    .WithEndpoint(port: 4318, targetPort: 4318, name: "http", scheme: "http", isProxied: false)
    .WithEnvironment("DOTNET_DASHBOARD_OTLP_HTTP_ENDPOINT_URL", $"http://{hostIp}:16175")
    .WithEnvironment("ASPIRE_API_KEY", "9d9ef33b72b8f63709748cff4916cbf7")
    .WithEnvironment("HONEYCOMB_API_KEY", Environment.GetEnvironmentVariable("HONEYCOMB_API_KEY") ?? throw new InvalidOperationException("HONEYCOMB_API_KEY is not configured."));

var pam = builder.AddProject<Projects.PAM>("PAM")
    .WithReference(dbPam)
    .WaitFor(dbPam)
    .WithReference(serviceBus)
    .WaitFor(serviceBus)
    .WithEnvironment("CUSTOM_OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4318");

var fpcc = builder.AddProject<Projects.FPCC>("FPCC")
    .WithReference(dbFpcc)
    .WaitFor(dbFpcc)
    .WithReference(serviceBus)
    .WaitFor(serviceBus)
    .WithEnvironment("CUSTOM_OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4318");

builder.Build().Run();