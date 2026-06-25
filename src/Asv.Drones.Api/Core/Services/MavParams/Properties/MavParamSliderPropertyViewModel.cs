using System.Globalization;
using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using R3;

namespace Asv.Drones.Api;

public sealed class MavParamSliderPropertyViewModel
    : PropertySliderViewModel,
        IMavParamPropertyViewModel,
        ISupportRefresh
{
    private readonly IMavParamContext _context;

    public const string TypeId = MavParamWidgetIds.Slider;

    public MavParamSliderPropertyViewModel(IMavParamContext context)
        : base(context.Info.Id, ToDouble(context.Info.Min), ToDouble(context.Info.Max))
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
        MavValue = Info.Metadata.DefaultValue;
        this.ApplyMavParamMetadata(Info, Client, Refresh);
        Units = Info.Metadata.Units;
        ValueFormat = Info.FormatString ?? ValueFormat;
        ConfigureIncrement();
        ApplyValueFromModel(ToDouble(Info.DefaultValue));

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

    protected override async ValueTask ApplyFromUser(double value, CancellationToken cancel)
    {
        var typedValue = ConvertToParamType(value);
        var error = Info.GetError(typedValue);
        if (!string.IsNullOrWhiteSpace(error))
        {
            throw new Exception(error);
        }

        var mavValue = Info.Convert(typedValue);
        await Client.WriteOnce(Info.Metadata.Name, mavValue, cancel);
        MavValue = mavValue;
    }

    private void ApplyRemoteValue(MavParamValue value)
    {
        MavValue = value;
        ApplyValueFromModel(ToDouble(Info.Convert(value)));
    }

    private void ConfigureIncrement()
    {
        var increment = ToDouble(Info.Increment);
        if (increment <= 0)
        {
            return;
        }

        TickFrequency = increment;
        SmallChange = increment;
        LargeChange = increment * 10d;
        IsSnapToTickEnabled = true;
    }

    private ValueType ConvertToParamType(double value)
    {
        return Info.Metadata.Type switch
        {
            MavParamType.MavParamTypeUint8 => Convert.ToByte(value),
            MavParamType.MavParamTypeInt8 => Convert.ToSByte(value),
            MavParamType.MavParamTypeUint16 => Convert.ToUInt16(value),
            MavParamType.MavParamTypeInt16 => Convert.ToInt16(value),
            MavParamType.MavParamTypeUint32 => Convert.ToUInt32(value),
            MavParamType.MavParamTypeInt32 => Convert.ToInt32(value),
            MavParamType.MavParamTypeReal32 => Convert.ToSingle(value),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private static double ToDouble(ValueType value)
    {
        return Convert.ToDouble(value, CultureInfo.InvariantCulture);
    }
}
