using System;
using System.Globalization;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class QuickParametersSliderViewModel : QuickParameterViewModel
{
    private decimal? _lastValue;
    private readonly IParamsClientEx _paramsClientEx;
    private readonly ParamDescription _description;

    public QuickParametersSliderViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();

        ParameterTitle = "Title";
        ParameterName = "PARAM_NAME";
        ParameterDescription = "Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter Some description of this parameter ";
        Range = "Range: [-128, 128]";
        Units = "Units: [Deg]";
        
        Minimum = -128;
        Maximum = 128;
        Increment = 1;
        Value = 22;
        
        _lastValue = Value;

        this.WhenAnyValue(vm => vm.Value)
            .Subscribe(val => IsChanged = val != _lastValue)
            .DisposeItWith(Disposable);
    }
    
    public QuickParametersSliderViewModel(string id, IParamsClientEx paramsClientEx, ParamDescription description) : base(id)
    {
        _paramsClientEx = paramsClientEx;   
        _description = description;
        
        ParameterTitle = description.DisplayName;
        ParameterName = description.Name;
        ParameterDescription = description.Description;

        Range = string.Format(RS.VehicleQuickParamSliderViewModel_Range, description.Min, description.Max);
        Units = string.Format(RS.VehicleQuickParamSliderViewModel_Units, description.Units);

        Minimum = description.Min;
        Maximum = description.Max;
        Increment = description.Increment;
        
        _lastValue = Value;
        
        this.WhenAnyValue(vm => vm.Value)
            .Skip(1)
            .Subscribe(val => {
                IsChanged = val != _lastValue;
                if (val < Minimum || val > Maximum)
                {
                    Value = _lastValue;
                }
            })
            .DisposeItWith(Disposable);
    }

    [Reactive] public string Range { get; set; }
    [Reactive] public string Units { get; set; }
    [Reactive] public decimal? Increment { get; set; }
    [Reactive] public decimal? Minimum { get; set; }
    [Reactive] public decimal? Maximum { get; set; }
    [Reactive] public decimal? Value { get; set; }
    
    public override async Task WriteParam(CancellationToken cancel)
    {
        switch (_description.ParamType)
        {
            case MavParamType.MavParamTypeUint8:
                _lastValue = (byte) await _paramsClientEx.WriteOnce(ParameterName, (byte)Value, cancel);
                break;
            case MavParamType.MavParamTypeInt8:
                _lastValue = (sbyte) await _paramsClientEx.WriteOnce(ParameterName, (sbyte)Value, cancel);
                break;
            case MavParamType.MavParamTypeUint16:
                _lastValue = (ushort) await _paramsClientEx.WriteOnce(ParameterName, (ushort)Value, cancel);
                break;
            case MavParamType.MavParamTypeInt16:
                _lastValue = (short) await _paramsClientEx.WriteOnce(ParameterName, (short)Value, cancel);
                break;
            case MavParamType.MavParamTypeUint32:
                _lastValue = (uint) await _paramsClientEx.WriteOnce(ParameterName, (uint)Value, cancel);
                break;
            case MavParamType.MavParamTypeInt32:
                _lastValue = (int) await _paramsClientEx.WriteOnce(ParameterName, (int)Value, cancel);
                break;
            case MavParamType.MavParamTypeReal32:
                _lastValue = (decimal)(float) await _paramsClientEx.WriteOnce(ParameterName, (float)Value, cancel);
                break;
            case MavParamType.MavParamTypeUint64:
            case MavParamType.MavParamTypeReal64:
            case MavParamType.MavParamTypeInt64:
            default:
                throw new ArgumentOutOfRangeException();
        }
        Value = _lastValue;
        IsChanged = false;
    }

    public override async Task ReadParam(CancellationToken cancel)
    {
        var mavParamValue = await _paramsClientEx.ReadOnce(ParameterName, cancel);
        _description.ParamType = mavParamValue.Type;
        switch (mavParamValue.Type)
        {
            case MavParamType.MavParamTypeUint8:
                _lastValue = (byte) mavParamValue;
                break;
            case MavParamType.MavParamTypeInt8:
                _lastValue = (sbyte) mavParamValue;
                break;
            case MavParamType.MavParamTypeUint16:
                _lastValue = (ushort) mavParamValue;
                break;
            case MavParamType.MavParamTypeInt16:
                _lastValue = (short) mavParamValue;
                break;
            case MavParamType.MavParamTypeUint32:
                _lastValue = (uint) mavParamValue;
                break;
            case MavParamType.MavParamTypeInt32:
                _lastValue = (int) mavParamValue;
                break;
            case MavParamType.MavParamTypeReal32:
                _lastValue = (decimal)(float) mavParamValue;
                break;
            case MavParamType.MavParamTypeUint64:
            case MavParamType.MavParamTypeReal64:
            case MavParamType.MavParamTypeInt64:
            default:
                throw new ArgumentOutOfRangeException();
        }
        Value = _lastValue;
        IsInitialized = true;
    }
}