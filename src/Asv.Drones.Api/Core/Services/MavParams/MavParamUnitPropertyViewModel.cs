using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using Asv.Modeling;
using R3;

namespace Asv.Drones.Api;

public abstract class MavParamUnitPropertyViewModel
    : PropertyUnitViewModel,
        IMavParamPropertyViewModel,
        ISupportRefresh
{
    private readonly IMavParamContext _context;
    private readonly Func<ValueType, double> _valueToSi;
    private readonly Func<double, ValueType> _siToValue;

    protected MavParamUnitPropertyViewModel(
        IMavParamContext context,
        IUnit unit,
        Func<ValueType, double> valueToSi,
        Func<double, ValueType> siToValue
    )
        : base(context.Info.Id, unit, context.Info.FormatString)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(unit);
        ArgumentNullException.ThrowIfNull(valueToSi);
        ArgumentNullException.ThrowIfNull(siToValue);

        _context = context;
        _valueToSi = valueToSi;
        _siToValue = siToValue;
        MavValue = Info.Metadata.DefaultValue;
        this.ApplyMavParamMetadata(Info, Client, Refresh);
        ApplyValueFromModel(_valueToSi(Info.DefaultValue));

        Client
            .Filter(Info.Metadata.Name)
            .ObserveOnCurrentSynchronizationContext()
            .Subscribe(
                ApplyRemoteValue,
                error =>
                {
                    if (error.Exception is not null)
                    {
                        ApplyErrorFromModel(error.Exception);
                    }
                }
            )
            .DisposeItWith(Disposable);

        MavParamPropertyEditorMetadata.ScheduleInitialRead(Refresh).DisposeItWith(Disposable);
    }

    public MavParamInfo Info => _context.Info;

    private IParamsClientEx Client => _context.Client;

    public MavParamValue MavValue
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
            ApplyRemoteValue(await Client.GetFromCacheOrReadOnce(Info.Metadata.Name, cancel));
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

    protected override Exception? ValidateSiValue(double siValue)
    {
        try
        {
            var value = _siToValue(siValue);
            var error = Info.GetError(value);
            return string.IsNullOrWhiteSpace(error) ? null : new Exception(error);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    protected override async ValueTask ApplyFromUser(double siValue, CancellationToken cancel)
    {
        var validationError = ValidateSiValue(siValue);
        if (validationError is not null)
        {
            throw validationError;
        }

        var value = _siToValue(siValue);
        var mavValue = Info.Convert(value);
        await Client.WriteOnce(Info.Metadata.Name, mavValue, cancel);
        MavValue = mavValue;
        ApplyValueFromModel(_valueToSi(value));
    }

    private void ApplyRemoteValue(MavParamValue value)
    {
        if (IsInEditMode)
        {
            return;
        }

        MavValue = value;
        ApplyValueFromModel(_valueToSi(Info.Convert(value)));
    }

    protected static IUnit GetUnit(IUnitService unitService, string unitId)
    {
        ArgumentNullException.ThrowIfNull(unitService);
        return unitService[unitId]
            ?? throw new InvalidOperationException($"Unit '{unitId}' was not found.");
    }
}

public sealed class MavParamAltitudePropertyViewModel : MavParamUnitPropertyViewModel
{
    public const string TypeId = nameof(MavParamWidgetType.Altitude);

    public MavParamAltitudePropertyViewModel(IMavParamContext context, IUnitService unitService)
        : base(
            context,
            GetUnit(unitService, AltitudeUnit.Id),
            value => MavlinkTypesHelper.AltFromMmToDoubleMeter(Convert.ToInt32(value)),
            value => MavlinkTypesHelper.AltFromDoubleMeterToInt32Mm(value)
        ) { }
}

public sealed class MavParamLatitudePropertyViewModel : MavParamUnitPropertyViewModel
{
    public const string TypeId = nameof(MavParamWidgetType.Latitude);

    public MavParamLatitudePropertyViewModel(IMavParamContext context, IUnitService unitService)
        : base(
            context,
            GetUnit(unitService, LatitudeUnit.Id),
            value => MavlinkTypesHelper.LatLonFromInt32E7ToDegDouble(Convert.ToInt32(value)),
            value => MavlinkTypesHelper.LatLonDegDoubleToFromInt32E7To(value)
        ) { }
}

public sealed class MavParamLongitudePropertyViewModel : MavParamUnitPropertyViewModel
{
    public const string TypeId = nameof(MavParamWidgetType.Longitude);

    public MavParamLongitudePropertyViewModel(IMavParamContext context, IUnitService unitService)
        : base(
            context,
            GetUnit(unitService, LongitudeUnit.Id),
            value => MavlinkTypesHelper.LatLonFromInt32E7ToDegDouble(Convert.ToInt32(value)),
            value => MavlinkTypesHelper.LatLonDegDoubleToFromInt32E7To(value)
        ) { }
}
