using Asv.Avalonia;

namespace Asv.Drones.Api;

public interface IAsyncFlightWidgetSection<in TContext> : IFlightWidgetSection
    where TContext : class
{
    ValueTask InitWithAsync(TContext context, CancellationToken cancel = default);
}

public interface IFlightWidgetSection<in TContext> : IFlightWidgetSection
    where TContext : class
{
    void InitWith(TContext context);
}

public interface IFlightWidgetSection : IRoutable
{
    int Order { get; }
}
