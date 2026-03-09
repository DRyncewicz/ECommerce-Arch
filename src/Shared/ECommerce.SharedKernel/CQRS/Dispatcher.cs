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

public sealed class Dispatcher(IServiceScopeFactory scopeFactory) : ICommandDispatcher, IQueryDispatcher
{
    public async Task<Result<TResult>> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        var handler = scope.ServiceProvider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod(nameof(ICommandHandler<ICommand<TResult>, TResult>.HandleAsync))!;
        return await (Task<Result<TResult>>)method.Invoke(handler, [command, ct])!;
    }

    public async Task<Result> SendAsync(ICommand command, CancellationToken ct = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        var handler = scope.ServiceProvider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod(nameof(ICommandHandler<ICommand>.HandleAsync))!;
        return await (Task<Result>)method.Invoke(handler, [command, ct])!;
    }

    public async Task<Result<TResult>> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = scope.ServiceProvider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod(nameof(IQueryHandler<IQuery<TResult>, TResult>.HandleAsync))!;
        return await (Task<Result<TResult>>)method.Invoke(handler, [query, ct])!;
    }
}
