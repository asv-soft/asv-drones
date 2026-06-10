using System.Collections.Generic;
using System.Windows.Input;
using Asv.Avalonia;
using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Drones;

public sealed class AddTelemetryDisplayItemViewModel : ViewModel
{
    public const string BaseId = "add-telemetry-display-item";

    public AddTelemetryDisplayItemViewModel()
        : base(BaseId)
    {
        AddCommand = new ReactiveCommand(
            async (_, ct) =>
                await this.Rise(new TelemetryDisplayItemAddRequestedEvent(this, ct), ct)
        ).DisposeItWith(Disposable);
    }

    public ICommand AddCommand { get; }

    public override IEnumerable<IViewModel> GetChildren() => [];
}
