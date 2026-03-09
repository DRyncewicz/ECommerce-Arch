using ECommerce.ProductService;
using ECommerce.SharedKernel.Endpoints;
using ECommerce.SharedKernel.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability("product-service");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProductServices(builder.Configuration);
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
