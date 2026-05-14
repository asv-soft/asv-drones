using System;
using System.Collections.Generic;
using System.Windows.Input;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class TelemetryDisplayItemViewModel : RoutableViewModel, ITelemetryDisplayItem
{
    public const string BaseId = "telemetry-display-item";

    public TelemetryDisplayItemViewModel(
        ITelemetryItem item,
        IReadOnlyBindableReactiveProperty<bool> isEditMode,
        ILoggerFactory loggerFactory
    )
        : base(
            new NavigationId(BaseId, item.ItemId ?? throw new ArgumentNullException(nameof(item))),
            loggerFactory
        )
    {
        Item = item;
        IsEditMode = isEditMode;
        RemoveCommand = new ReactiveCommand(
            async (_, ct) =>
                await this.Rise(new TelemetryDisplayItemRemoveRequestedEvent(this, ct), ct)
        ).DisposeItWith(Disposable);
    }

    public ITelemetryItem Item { get; }
    public ICommand RemoveCommand { get; }
    public IReadOnlyBindableReactiveProperty<bool> IsEditMode { get; }

    public override IEnumerable<IRoutable> GetChildren() => [];
}
