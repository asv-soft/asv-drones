using System;
using System.Collections.Generic;
using System.Linq;
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

public class QuickParametersComboBoxViewModel : QuickParameterViewModel
{
    private ParamDescriptionValue _lastValue;
    private readonly IParamsClientEx _paramsClientEx;
    private readonly ParamDescription _description;

    public QuickParametersComboBoxViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();

        ParameterTitle = "Title";
        ParameterName = "PARAM_NAME";
        ParameterDescription = "Some description of this parameter";

        Values =
        [
            new() { Description = "Some", Code = 0 },
            new() { Description = "Important", Code = 1 },
            new() { Description = "Value", Code = 2 },
        ];

        SelectedValue = Values.First();
        _lastValue = SelectedValue;

        this.WhenAnyValue(vm => vm.SelectedValue)
            .Subscribe(val => IsChanged = val.Code != _lastValue.Code)
            .DisposeItWith(Disposable);
    }

    public QuickParametersComboBoxViewModel(string id, IParamsClientEx paramsClientEx, ParamDescription description) : base(id)
    {
        _paramsClientEx = paramsClientEx;
        _description = description;

        ParameterTitle = description.DisplayName;
        ParameterName = description.Name;
        ParameterDescription = description.Description;

        Values = description.AvailableValues;

        SelectedValue = Values.First();
        _lastValue = SelectedValue;

        this.WhenAnyValue(vm => vm.SelectedValue)
            .Subscribe(val => IsChanged = val.Code != _lastValue.Code)
            .DisposeItWith(Disposable);
    }

    [Reactive] public ParamDescriptionValue SelectedValue { get; set; }

    public IEnumerable<ParamDescriptionValue> Values { get; }

    public override async Task WriteParam(CancellationToken cancel)
    {
        MavParamValue value;
        switch (_description.ParamType)
        {
            case MavParamType.MavParamTypeUint8:
                value = await _paramsClientEx.WriteOnce(ParameterName, (byte)SelectedValue.Code, cancel);
                _lastValue = Values.FirstOrDefault(_ => _.Code == (byte)value) ?? Values.First();
                break;
            case MavParamType.MavParamTypeInt8:
                value = await _paramsClientEx.WriteOnce(ParameterName, (sbyte)SelectedValue.Code, cancel);
                _lastValue = Values.FirstOrDefault(_ => _.Code == (sbyte)value) ?? Values.First();
                break;
            case MavParamType.MavParamTypeUint16:
                value = await _paramsClientEx.WriteOnce(ParameterName, (ushort)SelectedValue.Code, cancel);
                _lastValue = Values.FirstOrDefault(_ => _.Code == (ushort)value) ?? Values.First();
                break;
            case MavParamType.MavParamTypeInt16:
                value = await _paramsClientEx.WriteOnce(ParameterName, (short)SelectedValue.Code, cancel);
                _lastValue = Values.FirstOrDefault(_ => _.Code == (short)value) ?? Values.First();
                break;
            case MavParamType.MavParamTypeUint32:
                value = await _paramsClientEx.WriteOnce(ParameterName, (uint)SelectedValue.Code, cancel);
                _lastValue = Values.FirstOrDefault(_ => _.Code == (uint)value) ?? Values.First();
                break;
            case MavParamType.MavParamTypeInt32:
                value = await _paramsClientEx.WriteOnce(ParameterName, (int)SelectedValue.Code, cancel);
                _lastValue = Values.FirstOrDefault(_ => _.Code == (int)value) ?? Values.First();
                break;
            case MavParamType.MavParamTypeReal32:
                value = await _paramsClientEx.WriteOnce(ParameterName, (float)SelectedValue.Code, cancel);
                _lastValue = Values.FirstOrDefault(_ => _.Code == (decimal)(float)value) ?? Values.First();
                break;
            case MavParamType.MavParamTypeUint64:
            case MavParamType.MavParamTypeReal64:
            case MavParamType.MavParamTypeInt64:
            default:
                throw new ArgumentOutOfRangeException();
        }
        IsChanged = false;
    }

    public override async Task ReadParam(CancellationToken cancel)
    {
        var value = await _paramsClientEx.ReadOnce(ParameterName, cancel);
        _description.ParamType = value.Type;
        switch (_description.ParamType)
        {
            case MavParamType.MavParamTypeUint8:
                _lastValue = Values.FirstOrDefault(_ => _.Code == (byte)value) ?? Values.First();
                break;
            case MavParamType.MavParamTypeInt8:
                _lastValue = Values.FirstOrDefault(_ => _.Code == (sbyte)value) ?? Values.First();
                break;
            case MavParamType.MavParamTypeUint16:
                _lastValue = Values.FirstOrDefault(_ => _.Code == (ushort)value) ?? Values.First();
                break;
            case MavParamType.MavParamTypeInt16:
                _lastValue = Values.FirstOrDefault(_ => _.Code == (short)value) ?? Values.First();
                break;
            case MavParamType.MavParamTypeUint32:
                _lastValue = Values.FirstOrDefault(_ => _.Code == (uint)value) ?? Values.First();
                break;
            case MavParamType.MavParamTypeInt32:
                _lastValue = Values.FirstOrDefault(_ => _.Code == (int)value) ?? Values.First();
                break;
            case MavParamType.MavParamTypeReal32:
                _lastValue = Values.FirstOrDefault(_ => _.Code == (decimal)(float)value) ?? Values.First();
                break;
            case MavParamType.MavParamTypeUint64:
            case MavParamType.MavParamTypeReal64:
            case MavParamType.MavParamTypeInt64:
            default:
                throw new ArgumentOutOfRangeException();
        }
        SelectedValue = _lastValue;
        IsInitialized = true;
    }
}