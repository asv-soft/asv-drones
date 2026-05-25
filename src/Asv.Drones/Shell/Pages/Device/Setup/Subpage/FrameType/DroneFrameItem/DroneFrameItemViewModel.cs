using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using Asv.Modeling;
using R3;

namespace Asv.Drones;

public sealed class DroneFrameItemViewModel : ViewModel
{
    public const string BaseId = "frameItem";

    public DroneFrameItemViewModel()
        : this(NullDroneFrame.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        ApplyCommand = new ReactiveCommand(
            async (_, cancel) =>
            {
                if (!IsCurrent.Value)
                {
                    await Task.Delay(1000, cancel);
                }
            }
        ).DisposeItWith(Disposable);
    }

    public DroneFrameItemViewModel(IDroneFrame model)
        : base(BaseId, new NavArgs(("id", model.Id)))
    {
        Model = model;
        IsCurrent = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);

        ApplyCommand = new ReactiveCommand(
            async (_, ct) =>
            {
                if (!IsCurrent.Value)
                {
                    await this.Rise(new CurrentDroneFrameChangeEvent(this), ct);
                }
            }
        ).DisposeItWith(Disposable);
    }

    public IDroneFrame Model { get; }
    public BindableReactiveProperty<bool> IsCurrent { get; }
    public ReactiveCommand ApplyCommand { get; }

    public bool Filter(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        return Model.Id.Contains(searchText, StringComparison.InvariantCultureIgnoreCase);
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
