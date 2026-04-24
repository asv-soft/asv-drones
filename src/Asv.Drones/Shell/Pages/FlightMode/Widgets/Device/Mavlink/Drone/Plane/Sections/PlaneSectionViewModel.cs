using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;

namespace Asv.Drones.Plane;

public class PlaneSectionViewModel : RoutableViewModel, IFlightWidgetSection<ArduPlaneClientDevice>
{
    public const string SectionId = "plane-widget-section";

    public PlaneSectionViewModel()
        : this(DesignTime.LoggerFactory) { }

    public PlaneSectionViewModel(ILoggerFactory loggerFactory)
        : base(SectionId, loggerFactory)
    {
        Order = -1;
    }

    public string? Type
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Text
    {
        get;
        set => SetField(ref field, value);
    }

    public void InitWith(ArduPlaneClientDevice context)
    {
        Type = context.GetType().Name;
        Text = context.Name.CurrentValue;
    }

    public int Order { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }
}
