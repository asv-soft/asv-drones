using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Drones;

public sealed class DroneFrameItemViewModel : RoutableViewModel
{
    public const string BaseId = "frame-item";

    public DroneFrameItemViewModel()
        : this(NullDroneFrame.Instance, NullLoggerFactory.Instance)
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

    public DroneFrameItemViewModel(IDroneFrame model, ILoggerFactory loggerFactory)
        : base(new NavigationId(BaseId, model.Id), loggerFactory)
    {
        Model = model;
        IsCurrent = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);

        ApplyCommand = new ReactiveCommand(
            async (_, _) =>
            {
                if (!IsCurrent.Value)
                {
                    await Rise(new CurrentDroneFrameChangeEvent(this));
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

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
