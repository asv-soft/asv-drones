using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.Mavlink;

namespace Asv.Drones.Plane;

public class PlaneSectionViewModel : ViewModel, IFlightWidgetSection
{
    public const string SectionId = "plane-widget-section";

    public PlaneSectionViewModel()
        : base(SectionId)
    {
        DesignTime.ThrowIfNotDesignMode();
        Order = -1;
    }

    public PlaneSectionViewModel(ArduPlaneClientDevice device)
        : base(SectionId)
    {
        Order = -1;
        Type = device.GetType().Name;
        Text = device.Name.CurrentValue;
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

    public int Order { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
