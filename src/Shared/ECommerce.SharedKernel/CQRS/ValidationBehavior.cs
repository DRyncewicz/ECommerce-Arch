using ECommerce.SharedKernel.Results;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.SharedKernel.CQRS;

/// <summary>
/// Decorates any ICommandHandler or IQueryHandler with FluentValidation before execution.
/// Register via AddValidatedCommandHandler / AddValidatedQueryHandler extension methods.
/// </summary>
public sealed class ValidationCommandHandler<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner,
    IServiceProvider services)
    : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public async Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken ct = default)
    {
        var validator = services.GetService<IValidator<TCommand>>();
        if (validator is not null)
        {
            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
            {
                var error = Error.Validation(
                    validation.Errors[0].PropertyName,
                    validation.Errors[0].ErrorMessage);
                return Result.Failure<TResult>(error);
            }
        }
        return await inner.HandleAsync(command, ct);
    }
}

public sealed class ValidationQueryHandler<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> inner,
    IServiceProvider services)
    : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    public async Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken ct = default)
    {
        var validator = services.GetService<IValidator<TQuery>>();
        if (validator is not null)
        {
            var validation = await validator.ValidateAsync(query, ct);
            if (!validation.IsValid)
            {
                var error = Error.Validation(
                    validation.Errors[0].PropertyName,
                    validation.Errors[0].ErrorMessage);
                return Result.Failure<TResult>(error);
            }
        }
        return await inner.HandleAsync(query, ct);
    }
}
