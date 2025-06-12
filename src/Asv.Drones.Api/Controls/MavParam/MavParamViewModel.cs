using System.Diagnostics;
using System.Globalization;
using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using R3;

namespace Asv.Drones.Api;

public class MavParamViewModel : RoutableViewModel, ISupportRefresh, ISupportCancel
{
    public IMavParamTypeMetadata Metadata { get; }

    public MavParamViewModel(IMavParamTypeMetadata metadata, Observable<MavParamValue> update)
        : base(metadata.Name)
    {
        Metadata = metadata;

        update
            .ObserveOnCurrentSynchronizationContext()
            .Subscribe(InternalOnRemoteChanged)
            .DisposeItWith(Disposable);

        Value = new BindableReactiveProperty<ValueType>(
            InternalConvert(metadata.DefaultValue)
        ).DisposeItWith(Disposable);
        Value
            .Where(_ => IsRemoteChange == false)
            .Subscribe(_ => IsSync = false)
            .DisposeItWith(Disposable);

        Max = InternalConvert(metadata.MaxValue);
        Min = InternalConvert(metadata.MinValue);
        Increment = InternalConvert(metadata.Increment);
        IsSync = true;
    }

    private void InternalOnRemoteChanged(MavParamValue value)
    {
        if (IsInEditMode)
        {
            return;
        }
        IsRemoteChange = true;
        Value.OnNext(InternalConvert(value));
        IsSync = true;
        IsNetworkError = false;
        IsRemoteChange = false;
    }

    private MavParamValue InternalConvert(ValueType value)
    {
        return Metadata.Type switch
        {
            MavParamType.MavParamTypeUint8 => new MavParamValue(Convert.ToByte(value)),
            MavParamType.MavParamTypeInt8 => new MavParamValue(Convert.ToSByte(value)),
            MavParamType.MavParamTypeUint16 => new MavParamValue(Convert.ToUInt16(value)),
            MavParamType.MavParamTypeInt16 => new MavParamValue(Convert.ToInt16(value)),
            MavParamType.MavParamTypeUint32 => new MavParamValue(Convert.ToUInt32(value)),
            MavParamType.MavParamTypeInt32 => new MavParamValue(Convert.ToInt32(value)),
            MavParamType.MavParamTypeReal32 => new MavParamValue(Convert.ToSingle(value)),
            MavParamType.MavParamTypeUint64
            or MavParamType.MavParamTypeInt64
            or MavParamType.MavParamTypeReal64 => throw new ArgumentOutOfRangeException(),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private ValueType InternalConvert(MavParamValue value)
    {
        Debug.Assert(
            value.Type == Metadata.Type,
            $"Value type {value.Type} does not match metadata type {Metadata.Type} for param {Metadata.Name}"
        );
        switch (Metadata.Type)
        {
            case MavParamType.MavParamTypeUint8:
                return (byte)value;
            case MavParamType.MavParamTypeInt8:
                return (sbyte)value;
            case MavParamType.MavParamTypeUint16:
                return (ushort)value;
            case MavParamType.MavParamTypeInt16:
                return (short)value;
            case MavParamType.MavParamTypeUint32:
                return (uint)value;
            case MavParamType.MavParamTypeInt32:
                return (int)value;
            case MavParamType.MavParamTypeReal32:
                return (float)value;
            case MavParamType.MavParamTypeUint64:
            case MavParamType.MavParamTypeInt64:
            case MavParamType.MavParamTypeReal64:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected string? GetError(ValueType? value)
    {
        if (value == null)
        {
            return "Value is required";
        }
        switch (Metadata.Type)
        {
            case MavParamType.MavParamTypeUint8:
                var byteVal = Convert.ToByte(value);
                if (byteVal > Convert.ToByte(Max) || byteVal < Convert.ToByte(Min))
                {
                    return $"Value must be in range {Min}..{Max}";
                }
                break;
            case MavParamType.MavParamTypeInt8:
                var sbyteVal = Convert.ToSByte(value);
                if (sbyteVal > Convert.ToSByte(Max) || sbyteVal < Convert.ToSByte(Min))
                {
                    return $"Value must be in range {Min}..{Max}";
                }
                break;
            case MavParamType.MavParamTypeUint16:
                var ushortVal = Convert.ToUInt16(value);
                if (ushortVal > Convert.ToUInt16(Max) || ushortVal < Convert.ToUInt16(Min))
                {
                    return $"Value must be in range {Min}..{Max}";
                }
                break;
            case MavParamType.MavParamTypeInt16:
                var shortVal = Convert.ToInt16(value);
                if (shortVal > Convert.ToInt16(Max) || shortVal < Convert.ToInt16(Min))
                {
                    return $"Value must be in range {Min}..{Max}";
                }
                break;
            case MavParamType.MavParamTypeUint32:
                var uintVal = Convert.ToUInt32(value);
                if (uintVal > Convert.ToUInt32(Max) || uintVal < Convert.ToUInt32(Min))
                {
                    return $"Value must be in range {Min}..{Max}";
                }
                break;
            case MavParamType.MavParamTypeInt32:
                var intVal = Convert.ToInt32(value);
                if (intVal > Convert.ToInt32(Max) || intVal < Convert.ToInt32(Min))
                {
                    return $"Value must be in range {Min}..{Max}";
                }
                break;
            case MavParamType.MavParamTypeReal32:
                var floatVal = Convert.ToSingle(value);
                if (floatVal > Convert.ToSingle(Max) || floatVal < Convert.ToSingle(Min))
                {
                    return $"Value must be in range {Min}..{Max}";
                }
                break;
            case MavParamType.MavParamTypeUint64:
            case MavParamType.MavParamTypeInt64:
            case MavParamType.MavParamTypeReal64:
            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    protected bool IsValid(ValueType? value)
    {
        if (value == null)
        {
            return false;
        }
        switch (Metadata.Type)
        {
            case MavParamType.MavParamTypeUint8:
                var byteVal = Convert.ToByte(value);
                if (byteVal > Convert.ToByte(Max) || byteVal < Convert.ToByte(Min))
                {
                    return false;
                }
                break;
            case MavParamType.MavParamTypeInt8:
                var sbyteVal = Convert.ToSByte(value);
                if (sbyteVal > Convert.ToSByte(Max) || sbyteVal < Convert.ToSByte(Min))
                {
                    return false;
                }
                break;
            case MavParamType.MavParamTypeUint16:
                var ushortVal = Convert.ToUInt16(value);
                if (ushortVal > Convert.ToUInt16(Max) || ushortVal < Convert.ToUInt16(Min))
                {
                    return false;
                }
                break;
            case MavParamType.MavParamTypeInt16:
                var shortVal = Convert.ToInt16(value);
                if (shortVal > Convert.ToInt16(Max) || shortVal < Convert.ToInt16(Min))
                {
                    return false;
                }
                break;
            case MavParamType.MavParamTypeUint32:
                var uintVal = Convert.ToUInt32(value);
                if (uintVal > Convert.ToUInt32(Max) || uintVal < Convert.ToUInt32(Min))
                {
                    return false;
                }
                break;
            case MavParamType.MavParamTypeInt32:
                var intVal = Convert.ToInt32(value);
                if (intVal > Convert.ToInt32(Max) || intVal < Convert.ToInt32(Min))
                {
                    return false;
                }
                break;
            case MavParamType.MavParamTypeReal32:
                var floatVal = Convert.ToSingle(value);
                if (floatVal > Convert.ToSingle(Max) || floatVal < Convert.ToSingle(Min))
                {
                    return false;
                }
                break;
            case MavParamType.MavParamTypeUint64:
            case MavParamType.MavParamTypeInt64:
            case MavParamType.MavParamTypeReal64:
            default:
                throw new ArgumentOutOfRangeException();
        }
        return true;
    }

    protected string? ConvertToString(ValueType? value, string? formatString)
    {
        if (value == null)
        {
            return null;
        }

        if (formatString == null)
        {
            return value.ToString();
        }

        switch (Metadata.Type)
        {
            case MavParamType.MavParamTypeUint8:
                return ((byte)value).ToString(formatString, CultureInfo.InvariantCulture);
            case MavParamType.MavParamTypeInt8:
                return ((sbyte)value).ToString(formatString, CultureInfo.InvariantCulture);
            case MavParamType.MavParamTypeUint16:
                return ((ushort)value).ToString(formatString, CultureInfo.InvariantCulture);
            case MavParamType.MavParamTypeInt16:
                return ((short)value).ToString(formatString, CultureInfo.InvariantCulture);
            case MavParamType.MavParamTypeUint32:
                return ((uint)value).ToString(formatString, CultureInfo.InvariantCulture);
            case MavParamType.MavParamTypeInt32:
                return ((int)value).ToString(formatString, CultureInfo.InvariantCulture);
            case MavParamType.MavParamTypeReal32:
                return ((float)value).ToString(formatString, CultureInfo.InvariantCulture);
            case MavParamType.MavParamTypeUint64:
            case MavParamType.MavParamTypeInt64:
            case MavParamType.MavParamTypeReal64:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public async void Refresh()
    {
        try
        {
            IsBusy = true;
            IsNetworkError = false;
            NetworkErrorMessage = null;
            IsInEditMode = false;
            await Api.Commands.Mavlink.ReadParam(this, Metadata.Name);
        }
        catch (Exception e)
        {
            IsNetworkError = true;
            NetworkErrorMessage = e.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async void Write()
    {
        if (HasValidationErrors)
        {
            return;
        }
        var lastValue = IsInEditMode;
        try
        {
            NetworkErrorMessage = null;
            IsNetworkError = false;
            IsBusy = true;
            await Api.Commands.Mavlink.WriteParam(
                this,
                Metadata.Name,
                InternalConvert(Value.Value)
            );
        }
        catch (Exception e)
        {
            IsNetworkError = true;
            NetworkErrorMessage = e.Message;
        }
        finally
        {
            IsInEditMode = lastValue;
            IsBusy = false;
        }
    }

    public bool HasValidationErrors
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsFocused
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsNetworkError
    {
        get;
        set => SetField(ref field, value);
    }

    public string? NetworkErrorMessage
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsSync
    {
        get;
        set => SetField(ref field, value);
    }

    public BindableReactiveProperty<ValueType> Value { get; }

    public bool IsRemoteChange
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsInEditMode
    {
        get;
        set => SetField(ref field, value);
    }

    public ValueType Max
    {
        get;
        set => SetField(ref field, value);
    }

    public ValueType Min
    {
        get;
        set => SetField(ref field, value);
    }

    public ValueType Increment
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsBusy
    {
        get;
        set => SetField(ref field, value);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield break;
    }

    public void Cancel() { }
}
