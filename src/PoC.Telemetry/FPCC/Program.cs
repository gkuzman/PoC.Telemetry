using FPCC;
using FPCC.DB;
using FPCC.Services;
using Shared;
using Shared.Contracts;
using Shared.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenTelemetry(TracingExtensions.Source.Name);
builder.Services.AddServiceDiscovery();
builder.AddSqlServerDbContext<FpccDbContext>("fpcc-db");
builder.AddAzureServiceBusClient("servicebus");

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddServiceBusMessageHandler<InitiateWithdrawalMessage, InitiateWithdrawalMessageHandler>(
    queueOrTopicName: Const.WithdrawalIncomingQueueName);
builder.Services.AddHttpClient("PAM", client =>
{
    client.BaseAddress = new Uri("https+http://PAM");
}).AddServiceDiscovery();
builder.Services.AddScoped<IWithdrawalService, WithdrawalService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FpccDbContext>();
    var retries = 5;
    for (var i = 0; i < retries; i++)
    {
        try
        {
            db.Database.EnsureCreated();
            break;
        }
        catch (Exception ex) when (i < retries - 1)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "Database creation attempt {Attempt} failed, retrying...", i + 1);
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();