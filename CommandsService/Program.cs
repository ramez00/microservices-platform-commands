using System.Runtime.InteropServices;
using CommandsService.Data;
using Microsoft.EntityFrameworkCore;
using CommandsService.EventProcessing;
using CommandsService.AsyncDataServices;
using CommandsService.SyncDataServices.Grpc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddControllers(); 
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IPlatformDataClient, PlatformDataClient>();

// builder.Services.AddGrpcClient<PlatformService.GrpcPlatform.GrpcPlatformClient>(o =>
// {
//     var grpcPlatform = builder.Configuration["GrpcPlatform"];
//     o.Address = new Uri(grpcPlatform);
// });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem"));

builder.Services.AddScoped<ICommandRepo, CommandRepo>();
builder.Services.AddSingleton<IEventProcessor, EventProcessor>();
builder.Services.AddHostedService<CommandsService.AsyncDataServices.MessageBusSubscriber>();  // Register the MessageBusSubscriber as a hosted service

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers(); 

PrepDb.PrepPopulation(app);


app.Run();

