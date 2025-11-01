using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Drones;

public sealed class MotorFrameItemViewModel : RoutableViewModel
{
    public const string BaseId = "frame-item";

    public MotorFrameItemViewModel()
        : this(new FakeMotorFrame { Id = "frame-id-1" }, NullLoggerFactory.Instance) { }

    public MotorFrameItemViewModel(IMotorFrame model, ILoggerFactory loggerFactory)
        : base(new NavigationId(BaseId, model.Id), loggerFactory)
    {
        Model = model;
        IsCurrent = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
    }

    public IMotorFrame Model { get; }
    public BindableReactiveProperty<bool> IsCurrent { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
