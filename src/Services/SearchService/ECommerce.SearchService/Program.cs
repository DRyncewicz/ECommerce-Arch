using ECommerce.SearchService;
using ECommerce.SharedKernel.Endpoints;
using ECommerce.SharedKernel.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability("search-service");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSearchServices(builder.Configuration);
builder.Services.AddEndpoints(typeof(Program).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapObservability();
app.UseHttpsRedirection();
app.MapEndpoints();
app.Run();

public partial class Program;
