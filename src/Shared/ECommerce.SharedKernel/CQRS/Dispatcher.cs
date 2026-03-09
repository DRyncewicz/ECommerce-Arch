using ECommerce.SharedKernel.Results;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.SharedKernel.CQRS;

public interface ICommandDispatcher
{
    Task<Result<TResult>> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default);
    Task<Result> SendAsync(ICommand command, CancellationToken ct = default);
}

public interface IQueryDispatcher
{
    Task<Result<TResult>> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default);
}

public sealed class Dispatcher(IServiceProvider services) : ICommandDispatcher, IQueryDispatcher
{
    public Task<Result<TResult>> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        dynamic handler = services.GetRequiredService(handlerType);
        return handler.HandleAsync((dynamic)command, ct);
    }

    public Task<Result> SendAsync(ICommand command, CancellationToken ct = default)
    {
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        dynamic handler = services.GetRequiredService(handlerType);
        return handler.HandleAsync((dynamic)command, ct);
    }

    public Task<Result<TResult>> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        dynamic handler = services.GetRequiredService(handlerType);
        return handler.HandleAsync((dynamic)query, ct);
    }
}
