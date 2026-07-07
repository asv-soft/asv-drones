using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones.Api;

public sealed class MavParamGeoPointPropertyViewModel
    : PropertyGeoPointViewModel,
        IMavParamPropertyViewModel,
        ISupportRefresh
{
    public const string TypeId = "geo-point";

    private readonly IParamsClientEx _client;
    private readonly MavParamInfo _latitude;
    private readonly MavParamInfo _longitude;
    private readonly MavParamInfo _altitude;

    public MavParamGeoPointPropertyViewModel(
        MavParamInfo latitude,
        MavParamInfo longitude,
        MavParamInfo altitude,
        IParamsClientEx client,
        IUnitService unitService,
        IDialogService dialogService
    )
        : base(GetId(latitude, longitude, altitude), unitService, dialogService)
    {
        ArgumentNullException.ThrowIfNull(latitude);
        ArgumentNullException.ThrowIfNull(longitude);
        ArgumentNullException.ThrowIfNull(altitude);
        ArgumentNullException.ThrowIfNull(client);

        _latitude = latitude;
        _longitude = longitude;
        _altitude = altitude;
        _client = client;

        LatitudeMavValue = latitude.Metadata.DefaultValue;
        LongitudeMavValue = longitude.Metadata.DefaultValue;
        AltitudeMavValue = altitude.Metadata.DefaultValue;

        this.ApplyMavParamMetadata(Info);
        Icon =
            latitude.GroupIcon
            ?? longitude.GroupIcon
            ?? altitude.GroupIcon
            ?? MaterialIconKind.MapMarkerOutline;
        AddMenuItems();
        ApplyValueFromModel(GetCurrentGeoPoint());

        Client
            .Filter(latitude.Metadata.Name)
            .ObserveOnCurrentSynchronizationContext()
            .Subscribe(value => ApplyRemoteValue(GeoPointAxis.Latitude, value), ApplyRemoteError)
            .DisposeItWith(Disposable);
        Client
            .Filter(longitude.Metadata.Name)
            .ObserveOnCurrentSynchronizationContext()
            .Subscribe(value => ApplyRemoteValue(GeoPointAxis.Longitude, value), ApplyRemoteError)
            .DisposeItWith(Disposable);
        Client
            .Filter(altitude.Metadata.Name)
            .ObserveOnCurrentSynchronizationContext()
            .Subscribe(value => ApplyRemoteValue(GeoPointAxis.Altitude, value), ApplyRemoteError)
            .DisposeItWith(Disposable);

        MavParamPropertyEditorMetadata.ScheduleInitialRead(Refresh).DisposeItWith(Disposable);
    }

    public MavParamInfo Info => _latitude;

    private IParamsClientEx Client => _client;

    public MavParamValue LatitudeMavValue
    {
        get;
        private set => SetField(ref field, value);
    }

    public MavParamValue LongitudeMavValue
    {
        get;
        private set => SetField(ref field, value);
    }

    public MavParamValue AltitudeMavValue
    {
        get;
        private set => SetField(ref field, value);
    }

    public async ValueTask Refresh(CancellationToken cancel = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var latitude = await Client.GetFromCacheOrReadOnce(_latitude.Metadata.Name, cancel);
            var longitude = await Client.GetFromCacheOrReadOnce(_longitude.Metadata.Name, cancel);
            var altitude = await Client.GetFromCacheOrReadOnce(_altitude.Metadata.Name, cancel);

            ApplyRemoteValues(latitude, longitude, altitude);
        }
        catch (Exception e)
        {
            ApplyErrorFromModel(e);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected override async ValueTask ApplyFromUser(GeoPoint value, CancellationToken cancel)
    {
        var wasBusy = IsBusy;
        ClearModelErrors();
        IsBusy = true;
        try
        {
            var latitudeValue = MavlinkTypesHelper.LatLonDegDoubleToFromInt32E7To(value.Latitude);
            var longitudeValue = MavlinkTypesHelper.LatLonDegDoubleToFromInt32E7To(value.Longitude);
            var altitudeValue = MavlinkTypesHelper.AltFromDoubleMeterToInt32Mm(value.Altitude);

            Validate(_latitude, latitudeValue);
            Validate(_longitude, longitudeValue);
            Validate(_altitude, altitudeValue);

            var latitudeMavValue = _latitude.Convert((ValueType)latitudeValue);
            var longitudeMavValue = _longitude.Convert((ValueType)longitudeValue);
            var altitudeMavValue = _altitude.Convert((ValueType)altitudeValue);

            await Client.WriteOnce(_latitude.Metadata.Name, latitudeMavValue, cancel);
            await Client.WriteOnce(_longitude.Metadata.Name, longitudeMavValue, cancel);
            await Client.WriteOnce(_altitude.Metadata.Name, altitudeMavValue, cancel);

            LatitudeMavValue = latitudeMavValue;
            LongitudeMavValue = longitudeMavValue;
            AltitudeMavValue = altitudeMavValue;

            ApplyValueFromModel(value);
            MarkUpdated();
        }
        catch (Exception e)
        {
            ApplyErrorFromModel(e);
        }
        finally
        {
            IsBusy = wasBusy;
        }
    }

    private void AddMenuItems()
    {
        Menu.Add(
            new MenuItem($"{Id}.set-default", $"Set default ({GetDefaultValueDisplay()})")
            {
                Icon = MaterialIconKind.Restore,
                Order = 10,
                Command = new ReactiveCommand(
                    async (_, cancel) =>
                    {
                        await ApplyFromUser(GetDefaultGeoPoint(), cancel).ConfigureAwait(false);
                        await Refresh(cancel).ConfigureAwait(false);
                    }
                ),
            }
        );
        Menu.Add(
            new MenuItem($"{Id}.refresh", "Refresh")
            {
                Icon = MaterialIconKind.Refresh,
                Order = 20,
                Command = new ReactiveCommand(
                    async (_, cancel) => await Refresh(cancel).ConfigureAwait(false)
                ),
            }
        );
    }

    private void ApplyRemoteValues(
        MavParamValue latitude,
        MavParamValue longitude,
        MavParamValue altitude
    )
    {
        LatitudeMavValue = latitude;
        LongitudeMavValue = longitude;
        AltitudeMavValue = altitude;
        ClearModelErrors();
        ApplyValueFromModel(GetCurrentGeoPoint());
        MarkUpdated();
    }

    private void ApplyRemoteValue(GeoPointAxis axis, MavParamValue value)
    {
        switch (axis)
        {
            case GeoPointAxis.Latitude:
                LatitudeMavValue = value;
                break;
            case GeoPointAxis.Longitude:
                LongitudeMavValue = value;
                break;
            case GeoPointAxis.Altitude:
                AltitudeMavValue = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
        }

        ClearModelErrors();
        ApplyValueFromModel(GetCurrentGeoPoint());
        MarkUpdated();
    }

    private void ApplyRemoteError(Result error)
    {
        if (error.Exception is not null)
        {
            ApplyErrorFromModel(error.Exception);
        }
    }

    private GeoPoint GetCurrentGeoPoint()
    {
        return new GeoPoint(
            MavlinkTypesHelper.LatLonFromInt32E7ToDegDouble(
                Convert.ToInt32(_latitude.Convert(LatitudeMavValue))
            ),
            MavlinkTypesHelper.LatLonFromInt32E7ToDegDouble(
                Convert.ToInt32(_longitude.Convert(LongitudeMavValue))
            ),
            MavlinkTypesHelper.AltFromMmToDoubleMeter(
                Convert.ToInt32(_altitude.Convert(AltitudeMavValue))
            )
        );
    }

    private GeoPoint GetDefaultGeoPoint()
    {
        return new GeoPoint(
            MavlinkTypesHelper.LatLonFromInt32E7ToDegDouble(
                Convert.ToInt32(_latitude.DefaultValue)
            ),
            MavlinkTypesHelper.LatLonFromInt32E7ToDegDouble(
                Convert.ToInt32(_longitude.DefaultValue)
            ),
            MavlinkTypesHelper.AltFromMmToDoubleMeter(Convert.ToInt32(_altitude.DefaultValue))
        );
    }

    private string GetDefaultValueDisplay()
    {
        return string.Join(
            ", ",
            _latitude.Print(_latitude.DefaultValue),
            _longitude.Print(_longitude.DefaultValue),
            _altitude.Print(_altitude.DefaultValue)
        );
    }

    private static string GetId(
        MavParamInfo latitude,
        MavParamInfo longitude,
        MavParamInfo altitude
    )
    {
        ArgumentNullException.ThrowIfNull(latitude);
        ArgumentNullException.ThrowIfNull(longitude);
        ArgumentNullException.ThrowIfNull(altitude);

        return string.IsNullOrWhiteSpace(latitude.GroupBy)
            ? $"{latitude.Id}:{longitude.Id}:{altitude.Id}"
            : latitude.GroupBy;
    }

    private static void Validate(MavParamInfo info, ValueType value)
    {
        var error = info.GetError(value);
        if (string.IsNullOrWhiteSpace(error) == false)
        {
            throw new Exception(error);
        }
    }

    private enum GeoPointAxis
    {
        Latitude,
        Longitude,
        Altitude,
    }
}
