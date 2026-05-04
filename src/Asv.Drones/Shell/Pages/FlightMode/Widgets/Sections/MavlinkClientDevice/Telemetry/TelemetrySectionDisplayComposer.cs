using System.Collections.Generic;
using System.Linq;
using Asv.Drones.Api;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class TelemetrySectionDisplayComposer(ILoggerFactory loggerFactory)
{
    public IReadOnlyList<ITelemetryDisplayItem> Compose(
        IReadOnlyList<ITelemetryItem> items,
        IReadOnlyBindableReactiveProperty<bool> isEditMode,
        bool allAvailableItemsAdded
    )
    {
        var result = items
            .Select(
                ITelemetryDisplayItem (i) =>
                    new TelemetryDisplayItemViewModel(i, isEditMode, loggerFactory)
            )
            .ToList();

        var shouldShowAdd = !allAvailableItemsAdded && (isEditMode.Value || items.Count == 0);
        if (shouldShowAdd)
        {
            result.Add(new AddTelemetryDisplayItemViewModel(loggerFactory));
        }

        return result;
    }
}
