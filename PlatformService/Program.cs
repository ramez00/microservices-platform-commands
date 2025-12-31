using Microsoft.EntityFrameworkCore;
using PlatformService.Data;
using PlatformService.SyncDataServices.Http;
using PlatformService.AsyncDataServices;
using PlatformService.SyncDataServices.Grpc;
using System.IO;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();


builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();
builder.Services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();
builder.Services.AddGrpc();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("--> Using InMem Db");
    builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseInMemoryDatabase("InMem"));
}
else
{
    Console.WriteLine("--> Using SqlServer Db");
    builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("PlatformsConn")));
}


var app = builder.Build();

if(app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

PrepDb.PrepPopulation(app, app.Environment.IsProduction());

app.MapControllers();
app.MapGrpcService<GrpcPlatformService>();
app.MapGet("/protos/platforms.proto", async context =>
{
    await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
});


// app.Urls.Add("http://localhost:5000");

Console.WriteLine("**** ====> Command Service Endpoint: " +
    builder.Configuration["CommandService"]);

app.Run();
