using ECommerce.SharedKernel.Results;

namespace ECommerce.SharedKernel.CQRS;

public interface IQuery<TResult> : IRequest<Result<TResult>>;
