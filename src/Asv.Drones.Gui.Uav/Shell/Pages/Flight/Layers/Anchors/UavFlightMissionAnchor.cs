using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Avalonia.Media;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav;

public class UavFlightMissionAnchor : FlightAnchorBase
{
    private readonly MissionItem _missionItem;

    public UavFlightMissionAnchor(MissionItem missionItem, IVehicle vehicle) : base(vehicle,$"flight-mission/{missionItem.Index}")
    {
        Size = 16;
        OffsetX = OffsetXEnum.Center;
        OffsetY = OffsetYEnum.Bottom;
        Icon = ConvertIcon(missionItem.Command.Value);
        IconBrush = Brushes.OrangeRed;
        IsVisible = true;
        IsEditable = false;

        _missionItem = missionItem;
        missionItem.Location.Subscribe(_=>Location = _).DisposeItWith(Disposable);
        this.WhenAnyValue(_ => _.Location).Subscribe(missionItem.Location);
        Title = $"{missionItem.Index}";

        Disposable.AddAction(() =>
        {

        });
    }

    public MissionItem MissionItem => _missionItem;

    private static MaterialIconKind ConvertIcon(MavCmd missionItemCommand)
    {
        return missionItemCommand switch
        {
            MavCmd.MavCmdNavTakeoff => MaterialIconKind.MapMarkerUp,
            _ => MaterialIconKind.MapMarkerStar
        };
    }
}