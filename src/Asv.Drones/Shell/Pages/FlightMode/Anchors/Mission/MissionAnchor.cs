using System.Globalization;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class MissionAnchor : MapAnchor<MissionAnchor>, IMissionAnchor
{
    public const string MissionAnchorIdBase = "mission";
    public const string MissionIndexArgKey = "index";

    public MissionAnchor()
        : base(DesignTime.Id.TypeId, DesignTime.ExtensionService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MissionAnchor(
        DeviceId deviceId,
        MissionItem missionItem,
        AsvColorKind iconColor,
        IExtensionService ext
    )
        : base(MissionAnchorIdBase, BuildArgs(deviceId, missionItem.Index), ext)
    {
        DeviceId = deviceId;
        MissionItem = missionItem;
        Icon = MaterialIconKind.StarFourPointsSmall;
        IconColor = iconColor;
        CenterX = HorizontalOffset.Default;
        CenterY = VerticalOffset.Default;
        IsReadOnly = true;
        Header = missionItem.Index.ToString();

        missionItem
            .Location.DistinctUntilChanged()
            .ObserveOnUIThreadDispatcher()
            .Subscribe(location => Location = location)
            .DisposeItWith(Disposable);
    }

    public DeviceId DeviceId { get; }
    public ushort MissionIndex => MissionItem.Index;
    public MissionItem MissionItem { get; }

    private static NavArgs BuildArgs(DeviceId deviceId, ushort missionIndex)
    {
        return new NavArgs([
            new KeyValuePair<string, string?>(
                DevicePageViewModelMixin.ArgsDeviceIdKey,
                deviceId.AsString()
            ),
            new KeyValuePair<string, string?>(
                MissionIndexArgKey,
                missionIndex.ToString(CultureInfo.InvariantCulture)
            ),
        ]);
    }
}
