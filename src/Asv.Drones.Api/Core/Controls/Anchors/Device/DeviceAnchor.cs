using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Drones.Api;

public class DeviceAnchor<TContext> : MapAnchor<TContext>, IDeviceAnchor, IDeviceActionTarget
    where TContext : class, IDeviceAnchor
{
    public const string BaseId = "device";

    public DeviceAnchor()
        : base(DesignTime.Id.TypeId, DesignTime.ExtensionService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public DeviceAnchor(
        string id,
        IEnumerable<KeyValuePair<string, string?>> args,
        IDeviceManager mng,
        IClientDevice dev,
        IExtensionService ext
    )
        : base($"{BaseId}.{id}", BuildArgs(args, dev.Id), ext)
    {
        Device = dev;
        Icon = mng.GetIcon(dev.Id) ?? MaterialIconKind.Memory;
        IconColor = mng.GetDeviceColor(dev.Id);
        CenterX = DeviceIconMixin.GetIconCenterX(dev.Id);
        CenterY = DeviceIconMixin.GetIconCenterY(dev.Id);
        dev.Name.DistinctUntilChanged()
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200))
            .Subscribe(x => Header = x ?? string.Empty)
            .DisposeItWith(Disposable);
    }

    public IClientDevice Device { get; }

    private static NavArgs BuildArgs(
        IEnumerable<KeyValuePair<string, string?>> args,
        DeviceId deviceId
    )
    {
        var snapshot = args.Where(x => x.Key != DevicePageViewModelMixin.ArgsDeviceIdKey)
            .Append(
                new KeyValuePair<string, string?>(
                    DevicePageViewModelMixin.ArgsDeviceIdKey,
                    deviceId.ToString()
                )
            );

        return new NavArgs(snapshot);
    }
}
