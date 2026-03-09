using ECommerce.SharedKernel.CQRS;
using ECommerce.SharedKernel.Endpoints;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CQRS dispatcher
builder.Services.AddSingleton<Dispatcher>();
builder.Services.AddSingleton<ICommandDispatcher>(sp => sp.GetRequiredService<Dispatcher>());
builder.Services.AddSingleton<IQueryDispatcher>(sp => sp.GetRequiredService<Dispatcher>());

// Handlers
builder.Services.AddScoped<
    ICommandHandler<ECommerce.OrderService.Features.PlaceOrder.PlaceOrderCommand, Guid>,
    ECommerce.OrderService.Features.PlaceOrder.PlaceOrderHandler>();

// Validators
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddEndpoints(typeof(Program).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapEndpoints();

app.Run();

public partial class Program;
