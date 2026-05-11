using FPCC.DB;
using FPCC.Services;
using Shared;
using Shared.Contracts;
using Shared.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenTelemetry("FPCC");
builder.AddSqlServerDbContext<FpccDbContext>("fpcc-db");
builder.AddAzureServiceBusClient("servicebus");

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddServiceBusMessageHandler<InitiateWithdrawalMessage, InitiateWithdrawalMessageHandler>(
    queueOrTopicName: Const.WithdrawalIncomingQueueName);
builder.Services.AddScoped<IWithdrawalService, WithdrawalService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FpccDbContext>();
    db.Database.EnsureCreated();
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