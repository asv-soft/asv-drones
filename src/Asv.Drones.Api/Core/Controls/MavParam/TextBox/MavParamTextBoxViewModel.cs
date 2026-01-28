using System.ComponentModel;
using System.Globalization;
using Asv.Avalonia;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones.Api;

public class MavParamTextBoxViewModel : MavParamViewModel
{
    private readonly BindableReactiveProperty<string> _textValue;
    private bool _internalChange;

    public MavParamTextBoxViewModel()
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
                }
            ),
            new Subject<MavParamValue>(),
            (_, _) => ValueTask.FromResult(new MavParamValue(100)),
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();

        Task.Run(async () =>
        {
            while (true)
            {
                IsSync = false;
                await Task.Delay(3000);

                IsNetworkError = true;
                NetworkErrorMessage = "Network error occurred. Please try again later.";
                await Task.Delay(5000);
                IsNetworkError = false;
                IsBusy = true;
                await Task.Delay(3000);
                IsBusy = false;
                IsRemoteChange = true;
                await Task.Delay(1000);
                IsRemoteChange = false;
                _textValue.Value = "asdasdasd";
                await Task.Delay(3000);
                _textValue.Value = "123450";
            }
        });
    }

    public MavParamTextBoxViewModel(
        MavParamInfo param,
        Observable<MavParamValue> update,
        InitialReadParamDelegate initReadCallback,
        ILoggerFactory loggerFactory
    )
        : base(param, update, initReadCallback, loggerFactory)
    {
        _textValue = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        _internalChange = true;
        Value
            .Where(_ => _internalChange == false)
            .Subscribe(x => _textValue.Value = ValueToText(x))
            .DisposeItWith(Disposable);

        // we don't subscribe to value changes here, because we set Value at Validator
        TextValue.EnableValidation(Validator).DisposeItWith(Disposable);
        Observable
            .FromEventHandler<DataErrorsChangedEventArgs>(
                h => _textValue.ErrorsChanged += h,
                h => _textValue.ErrorsChanged -= h
            )
            .Subscribe(_ => HasValidationErrors = _textValue.HasErrors)
            .DisposeItWith(Disposable);

        _textValue.DistinctUntilChanged().Subscribe(_ => IsSync = false).DisposeItWith(Disposable);
        _internalChange = false;
    }

    public virtual string? Units => Info.Metadata.Units;

    protected virtual string ValueToText(ValueType remoteValue)
    {
        return Info.Print(remoteValue) ?? string.Empty;
    }

    protected virtual Exception? TextToValue(string valueAsString, out ValueType value)
    {
        return Info.ValidateString(valueAsString, out value);
    }

    private Exception? Validator(string valueAsString)
    {
        var err = TextToValue(valueAsString, out var value);
        if (err != null)
        {
            return err;
        }
        _internalChange = true;
        Value.Value = value;
        _internalChange = false;
        return null;
    }

    public IReadOnlyBindableReactiveProperty<string> TextValue => _textValue;
}
