using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones.Api;

public class MavParamComboBoxViewModel : MavParamViewModel
{
    private bool _internalChange;

    public MavParamComboBoxViewModel()
        : this(
            new MavParamInfo(
                new MavParamTypeMetadata(
                    "A"
                        + NavigationId
                            .GenerateRandomAsString(15)
                            .Replace('.', '_')
                            .Replace('-', '_'),
                    MavParamType.MavParamTypeInt32
                )
                {
                    Units = "MHz",
                    RebootRequired = false,
                    Volatile = false,
                    MinValue = new MavParamValue(-100),
                    ShortDesc = "Test param",
                    LongDesc = "Long description for test param",
                    Group = "System",
                    Category = "System",
                    MaxValue = new MavParamValue(100),
                    DefaultValue = new MavParamValue(50),
                    Increment = new MavParamValue(1),
                    Values =
                    [
                        new ValueTuple<MavParamValue, string>(
                            1,
                            "TX1 [icon=numeric-1-box-outline]"
                        ),
                        new ValueTuple<MavParamValue, string>(
                            2,
                            "TX2 [icon=numeric-2-box-outline]"
                        ),
                    ],
                }
            ),
            new Subject<MavParamValue>(),
            (_, _) => ValueTask.FromResult(new MavParamValue(100)),
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MavParamComboBoxViewModel(
        MavParamInfo info,
        Observable<MavParamValue> update,
        InitialReadParamDelegate initReadCallback,
        ILoggerFactory loggerFactory
    )
        : base(info, update, initReadCallback, loggerFactory)
    {
        Items = info.GetPredefinedValues().ToArray();
        Value
            .Where(_ => _internalChange == false)
            .Subscribe(x =>
            {
                _internalChange = true;
                SelectedItem = Items.FirstOrDefault(i => i.Value.Equals(x));
                _internalChange = false;
            })
            .DisposeItWith(Disposable);

        this.ObservePropertyChanged(x => x.SelectedItem, pushCurrentValueOnSubscribe: false)
            .Where(_ => _internalChange == false)
            .Subscribe(x =>
            {
                if (x == null)
                {
                    return;
                }

                _internalChange = true;
                Value.Value = x.Value;
                _internalChange = false;
                Write();
            })
            .DisposeItWith(Disposable);
    }

    public MavParamValueItem? SelectedItem
    {
        get;
        set => SetField(ref field, value);
    }

    public MavParamValueItem[] Items { get; }
}
