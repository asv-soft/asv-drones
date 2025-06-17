using System.ComponentModel;
using System.Globalization;
using Asv.Avalonia;
using Asv.Common;
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
            new MavParamTypeMetadata(NavigationId.GenerateRandomAsString(16), MavParamType.MavParamTypeInt32)
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
        }, new Subject<MavParamValue>(), 
            DesignTime.LoggerFactory)
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
    
    public MavParamTextBoxViewModel(IMavParamTypeMetadata param, Observable<MavParamValue> update, ILoggerFactory loggerFactory, string? formatString = null) 
        : base(param, update, loggerFactory)
    {
        _textValue = new BindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        
        Value.Where(_ => _internalChange == false)
            .Subscribe(x => _textValue.Value = ConvertToString(x, formatString) ?? string.Empty)
            .DisposeItWith(Disposable);

        // we don't subscribe to value changes here, because we set Value at Validator
        TextValue
            .EnableValidation(Validator)
            .DisposeItWith(Disposable);
        Observable.FromEventHandler<DataErrorsChangedEventArgs>(
                h => _textValue.ErrorsChanged += h,
                h => _textValue.ErrorsChanged -= h)
            .Subscribe(_ => HasValidationErrors = _textValue.HasErrors)
            .DisposeItWith(Disposable);

        _textValue.DistinctUntilChanged()
            .Subscribe(_ => IsSync = false)
            .DisposeItWith(Disposable);
    }

    private Exception? Validator(string valueAsString)
    {
        if (string.IsNullOrWhiteSpace(valueAsString))
        {
            return new Exception("Value is empty");
        }
        valueAsString = valueAsString.Replace(',', '.').Trim(' ').Replace(" ", string.Empty);
        if (valueAsString.Length == 0)
        {
            return new Exception("Value is empty");
        }
        var lastChar = valueAsString[^1];
        int multiply;
        switch (lastChar)
        {
            case 'M' or 'm' or 'М' or 'м':
                multiply = 1_000_000;
                valueAsString = valueAsString[..^1];
                break;
            case 'K' or 'k' or 'К' or 'к':
                multiply = 1_000;
                valueAsString = valueAsString[..^1];
                break;
            case 'G' or 'g' or 'Г' or 'г':
                multiply = 1_000_000_000;
                valueAsString = valueAsString[..^1];
                break;
            default:
                multiply = 1;
                break;
        }
        
        if (double.TryParse(valueAsString, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
        {
            doubleValue *= multiply;
        }
        else
        {
           return new Exception("Value must be a number with optional suffix (K, M, G)");
        }

        ValueType value;
        switch (Metadata.Type)
        {
            case MavParamType.MavParamTypeUint8:
                value = (byte)doubleValue;
                break;
            case MavParamType.MavParamTypeInt8:
                value = (sbyte)doubleValue;
                break;
            case MavParamType.MavParamTypeUint16:
                value = (ushort)doubleValue;
                break;
                
            case MavParamType.MavParamTypeInt16:
                value = (short)doubleValue;
                break;
                
            case MavParamType.MavParamTypeUint32:
                value = (uint)doubleValue;
                break;           
            case MavParamType.MavParamTypeInt32:
                value = (int)doubleValue;
                break;
            case MavParamType.MavParamTypeReal32:
                value = (float)doubleValue;
                break;
            case MavParamType.MavParamTypeReal64:
            case MavParamType.MavParamTypeUint64:
            case MavParamType.MavParamTypeInt64:
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (IsValid(value) == false)
        {
            return new Exception(GetError(value));
        }

        _internalChange = true;
        Value.Value = value;
        _internalChange = false;
        return null;
    }

    public IReadOnlyBindableReactiveProperty<string> TextValue => _textValue;

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield break;
    }

}