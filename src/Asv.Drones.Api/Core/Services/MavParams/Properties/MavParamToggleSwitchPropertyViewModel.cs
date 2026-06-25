using System.Globalization;
using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones.Api;

public sealed class MavParamToggleSwitchPropertyViewModel
    : PropertyToggleSwitchViewModel,
        IMavParamPropertyViewModel,
        ISupportRefresh
{
    private readonly IMavParamContext _context;
    private readonly MavParamValue _checkedValue;
    private readonly MavParamValue _uncheckedValue;

    public const string TypeId = MavParamWidgetIds.ToggleSwitch;

    public MavParamToggleSwitchPropertyViewModel(IMavParamContext context)
        : base(context.Info.Id)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
        MavValue = Info.Metadata.DefaultValue;
        this.ApplyMavParamMetadata(Info, Client, Refresh);

        var predefinedValues = Info.GetPredefinedValues().ToArray();
        var uncheckedItem = predefinedValues.FirstOrDefault(item =>
            ValueToBool(item.Value) == false
        );
        var checkedItem = predefinedValues.FirstOrDefault(item => ValueToBool(item.Value));

        _uncheckedValue = uncheckedItem?.MavValue ?? Info.Convert((ValueType)0);
        _checkedValue = checkedItem?.MavValue ?? Info.Convert((ValueType)1);

        ApplyStateMetadata(checkedItem, true);
        ApplyStateMetadata(uncheckedItem, false);
        ApplyValueFromModel(ValueToBool(Info.DefaultValue));

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

    protected override async ValueTask ApplyFromUser(bool value, CancellationToken cancel)
    {
        var mavValue = value ? _checkedValue : _uncheckedValue;
        Validate(mavValue);
        await Client.WriteOnce(Info.Metadata.Name, mavValue, cancel);
        MavValue = mavValue;
    }

    private void ApplyRemoteValue(MavParamValue value)
    {
        MavValue = value;
        ApplyValueFromModel(ValueToBool(Info.Convert(value)));
    }

    private void ApplyStateMetadata(MavParamValueItem? item, bool isChecked)
    {
        if (item is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(item.Title))
        {
            if (isChecked)
            {
                CheckedText = item.Title;
            }
            else
            {
                UncheckedText = item.Title;
            }
        }

        if (item.Icon is { } icon)
        {
            SetIcon(icon, isChecked);
        }

        if (item.IconColor != AsvColorKind.None)
        {
            SetStatus(item.IconColor, isChecked);
        }
    }

    private void SetIcon(MaterialIconKind icon, bool isChecked)
    {
        if (isChecked)
        {
            CheckedIcon = icon;
        }
        else
        {
            UncheckedIcon = icon;
        }
    }

    private void SetStatus(AsvColorKind status, bool isChecked)
    {
        if (isChecked)
        {
            CheckedStatus = status;
        }
        else
        {
            UncheckedStatus = status;
        }
    }

    private void Validate(MavParamValue value)
    {
        var error = Info.GetError(Info.Convert(value));
        if (!string.IsNullOrWhiteSpace(error))
        {
            throw new Exception(error);
        }
    }

    private static bool ValueToBool(ValueType value)
    {
        return Convert.ToDouble(value, CultureInfo.InvariantCulture) != 0d;
    }
}
