using System.Collections.Generic;
using System.Windows.Input;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.Modeling;
using R3;

namespace Asv.Drones;

public sealed class TelemetryDisplayItemViewModel : ViewModel
{
    public const string BaseId = "telemetry-display-item";

    public TelemetryDisplayItemViewModel(
        IRttBoxViewModel item,
        IReadOnlyBindableReactiveProperty<bool> isEditMode
    )
        : base(BaseId, new NavArgs(("id", item.Id.ToString())))
    {
        Item = item.SetRoutableParent(this);
        IsEditMode = isEditMode;
        RemoveCommand = new ReactiveCommand(
            async (_, ct) =>
                await this.Rise(new TelemetryDisplayItemRemoveRequestedEvent(this, ct), ct)
        ).DisposeItWith(Disposable);
    }

    public IRttBoxViewModel Item { get; }
    public string ItemId => Item.Id.ToString();
    public ICommand RemoveCommand { get; }
    public IReadOnlyBindableReactiveProperty<bool> IsEditMode { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Item;
    }
}
