namespace ECommerce.SharedKernel.Messaging;

public interface IEventBus
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : IIntegrationEvent;
}
