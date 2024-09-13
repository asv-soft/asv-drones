using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using DynamicData;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class BitmaskPart : DisposableReactiveObject
{
    public BitmaskPart(string key, bool value)
    {
        Key = key;
        Value = value;
    }

    [Reactive] public string Key { get; set; } = string.Empty;
    [Reactive] public bool Value { get; set; }
}

public class QuickParametersBitmaskViewModel : QuickParameterViewModel
{
    private readonly SourceList<BitmaskPart> _partsList = new();
    private ReadOnlyObservableCollection<BitmaskPart> _parts;
    private bool[] _lastValues;
    private readonly IParamsClientEx _paramsClientEx;
    private readonly ParamDescription _description;

    public QuickParametersBitmaskViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();

        ParameterTitle = "Title";
        ParameterName = "PARAM_NAME";
        ParameterDescription = "Some description of this parameter";

        IEnumerable<BitmaskPart> startItems =
        [
            new BitmaskPart("Some", false),
            new BitmaskPart("Values", false),
            new BitmaskPart("Of", true),
            new BitmaskPart("Bitmask", true),
            new BitmaskPart("Some", false),
            new BitmaskPart("Values", false),
            new BitmaskPart("Of", true),
            new BitmaskPart("Bitmask", true)
        ];

        var values = startItems.Select(item => item.Value).ToArray();

        _partsList.Connect()
            .AutoRefresh()
            .Bind(out _parts)
            .Subscribe(_ =>
            {
                IsChanged = !_parts.Select(item => item.Value).SequenceEqual(values);
            })
            .DisposeItWith(Disposable);

        _partsList.AddRange(startItems);
    }
    
    public QuickParametersBitmaskViewModel(string id, IParamsClientEx paramsClientEx, ParamDescription description) : base(id)
    {
        _paramsClientEx = paramsClientEx;
        _description = description;
        
        ParameterTitle = description.DisplayName;
        ParameterName = description.Name;
        ParameterDescription = description.Description;
        
        _lastValues = new bool[description.AvailableValues.Count];
        
        _partsList.Connect()
            .AutoRefresh()
            .Bind(out _parts)
            .Subscribe(_ =>
            {
                IsChanged = !_parts.Select(item => item.Value)
                    .SequenceEqual(_lastValues.Take(_parts.Count));
            }).DisposeItWith(Disposable);
    }

    public ReadOnlyObservableCollection<BitmaskPart> Parts => _parts;

    private BitArray GetBitArrayFromMavParamValue(MavParamValue value) => value.Type switch
    {
        MavParamType.MavParamTypeInt8 => new BitArray( new []{(byte)value}),
        MavParamType.MavParamTypeUint8 => new BitArray(new []{(byte)value}),
        MavParamType.MavParamTypeUint16 => new BitArray(BitConverter.GetBytes((ushort)value)),
        MavParamType.MavParamTypeInt16 => new BitArray(BitConverter.GetBytes((short)value)),
        MavParamType.MavParamTypeUint32 => new BitArray(BitConverter.GetBytes((uint)value)),
        MavParamType.MavParamTypeInt32 => new BitArray(BitConverter.GetBytes((int)value)),
        MavParamType.MavParamTypeReal32 => new BitArray(BitConverter.GetBytes((float)value)),
        _ => throw new ArgumentOutOfRangeException()
    };

    private MavParamValue GetMavParamValueFromBitArray(BitArray value)
    {
        byte[] tempArray;
        switch (_description.ParamType)
        {
            case MavParamType.MavParamTypeUint8:
                tempArray = new byte[1];
                value.CopyTo(tempArray, 0);
                return new MavParamValue((sbyte)tempArray[0]);
            case MavParamType.MavParamTypeInt8:
                tempArray = new byte[1];
                value.CopyTo(tempArray, 0);
                return new MavParamValue(tempArray[0]);
            case MavParamType.MavParamTypeUint16:
                tempArray = new byte[2];
                value.CopyTo(tempArray, 0);
                return new MavParamValue(BitConverter.ToUInt16(tempArray));
            case MavParamType.MavParamTypeInt16:
                tempArray = new Byte[2];
                value.CopyTo(tempArray, 0);
                return new MavParamValue(BitConverter.ToInt16(tempArray));
            case MavParamType.MavParamTypeUint32:
                tempArray = new Byte[4];
                value.CopyTo(tempArray, 0);
                return new MavParamValue(BitConverter.ToUInt32(tempArray));
            case MavParamType.MavParamTypeInt32:
                tempArray = new Byte[4];
                value.CopyTo(tempArray, 0);
                return new MavParamValue(BitConverter.ToInt32(tempArray));
            case MavParamType.MavParamTypeReal32:
                tempArray = new Byte[4];
                value.CopyTo(tempArray, 0);
                return new MavParamValue(BitConverter.ToSingle(tempArray));
            case MavParamType.MavParamTypeUint64:
            case MavParamType.MavParamTypeInt64:
            case MavParamType.MavParamTypeReal64:
            default:
                throw new ArgumentOutOfRangeException();
        }
    } 
    
    public override async Task WriteParam(CancellationToken cancel)
    {
        var bitArray = new BitArray(_parts.Select(part => part.Value).ToArray());
        await _paramsClientEx.WriteOnce(ParameterName, GetMavParamValueFromBitArray(bitArray), cancel);;
        _lastValues = _parts.Select(part => part.Value).ToArray();
        IsChanged = false;
    }

    public override async Task ReadParam(CancellationToken cancel)
    {
        var value = await _paramsClientEx.ReadOnce(ParameterName, cancel);
        _description.ParamType = value.Type;
        var bitArray = GetBitArrayFromMavParamValue(value);

        _lastValues = new bool[bitArray.Length];
        _partsList.Clear();
        
        for (int i = 0; i < _description.AvailableValues.Count; i++)
        {
            _lastValues[i] = bitArray[i];
            _partsList.Add(new BitmaskPart(_description.AvailableValues[i].Description, _lastValues[i]));
        }
        IsInitialized = true;
    }
}