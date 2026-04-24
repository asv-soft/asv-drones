using System;
using System.Collections.Generic;
using Asv.Drones.Api;
using Asv.IO;

namespace Asv.Drones;

public class ClientDeviceWidgetFactory : IClientDeviceWidgetFactory
{
    private readonly IReadOnlyDictionary<Type, IClientDeviceWidgetCreationHandler> _handlers;

    public ClientDeviceWidgetFactory(IEnumerable<IClientDeviceWidgetCreationHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);

        var result = new Dictionary<Type, IClientDeviceWidgetCreationHandler>();
        foreach (var handler in handlers)
        {
            if (!result.TryAdd(handler.DeviceType, handler))
            {
                throw new ArgumentException(
                    $"Duplicate widget handler for type '{handler.DeviceType.FullName}'."
                );
            }
        }

        _handlers = result;
    }

    public IFlightWidget? CreateWidget(in IClientDevice device)
    {
        var currentType = device.GetType();
        while (currentType is not null)
        {
            if (_handlers.TryGetValue(currentType, out var handler))
            {
                return handler.Create(in device);
            }

            currentType = currentType.BaseType;
        }

        return null;
    }
}
