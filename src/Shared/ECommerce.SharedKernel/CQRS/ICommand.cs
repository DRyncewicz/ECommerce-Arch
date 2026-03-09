using ECommerce.SharedKernel.Results;

namespace ECommerce.SharedKernel.CQRS;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResult> : IRequest<Result<TResult>>;

public interface IRequest<TResponse>;
