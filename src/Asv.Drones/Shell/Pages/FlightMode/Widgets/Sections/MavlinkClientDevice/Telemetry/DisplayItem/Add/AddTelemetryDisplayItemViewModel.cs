using System.Collections.Generic;
using System.Windows.Input;
using Asv.Avalonia;
using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class AddTelemetryDisplayItemViewModel : RoutableViewModel, ITelemetryDisplayItem
{
    public const string BaseId = "add-telemetry-display-item";

    public AddTelemetryDisplayItemViewModel(ILoggerFactory loggerFactory)
        : base(BaseId, loggerFactory)
    {
        AddCommand = new ReactiveCommand(
            async (_, ct) =>
                await this.Rise(new TelemetryDisplayItemAddRequestedEvent(this, ct), ct)
        ).DisposeItWith(Disposable);
    }

    public ICommand AddCommand { get; }

    public override IEnumerable<IRoutable> GetChildren() => [];
}
