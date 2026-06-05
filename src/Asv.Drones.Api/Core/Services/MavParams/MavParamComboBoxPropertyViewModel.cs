using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using Asv.Modeling;
using R3;

namespace Asv.Drones.Api;

public sealed class MavParamComboBoxPropertyViewModel
    : PropertyComboBoxViewModel,
        IMavParamPropertyViewModel,
        ISupportRefresh
{
    private readonly IMavParamContext _context;

    public const string TypeId = nameof(MavParamWidgetType.ComboBox);

    public MavParamComboBoxPropertyViewModel(IMavParamContext context)
        : base(context.Info.Id)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
        MavValue = Info.Metadata.DefaultValue;
        this.ApplyMavParamMetadata(Info, Client, Refresh);

        foreach (var item in Info.GetPredefinedValues())
        {
            ItemsSource.Add(
                new MavParamValueItemViewModel(
                    NavId.GenerateByHashAsString(Info.Id, item.MavValue.ToString()),
                    item
                )
            );
        }

        SelectedItem.Value = FindItem(MavValue);

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

    protected override async ValueTask ApplyFromUser(
        IHeadlinedViewModel item,
        CancellationToken cancel
    )
    {
        if (item is not MavParamValueItemViewModel mavParamItem)
        {
            throw new ArgumentException(
                $"Item must be of type {nameof(MavParamValueItemViewModel)}.",
                nameof(item)
            );
        }

        await Client.WriteOnce(Info.Metadata.Name, mavParamItem.MavValue, cancel);
        MavValue = mavParamItem.MavValue;
    }

    private void ApplyRemoteValue(MavParamValue value)
    {
        MavValue = value;
        ApplyValueFromModel(FindItem(value));
    }

    private MavParamValueItemViewModel? FindItem(MavParamValue value)
    {
        var typedValue = Info.Convert(value);
        return ItemsSource
            .OfType<MavParamValueItemViewModel>()
            .FirstOrDefault(item => Equals(item.Value, typedValue));
    }
}

public sealed class MavParamValueItemViewModel : HeadlinedViewModel
{
    public MavParamValueItemViewModel(string id, MavParamValueItem item)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(item);

        MavValue = item.MavValue;
        Value = item.Value;
        Header = item.Title;
        Description = item.Description;
        Icon = item.Icon;
        IconColor = item.IconColor;
    }

    public MavParamValue MavValue { get; }

    public ValueType Value { get; }
}
