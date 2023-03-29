using System.Reactive;
using System.Reactive.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.Client;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class ParametersEditorParameterViewModel : ViewModelBase
{

    private readonly IVehicle _vehicle;
    public const string UriString = ShellMenuItem.UriString + ".parameter";
    public static readonly Uri Uri = new Uri(UriString);
    
    public ParametersEditorParameterViewModel() : base(Uri)
    {
        
    }

    public ParametersEditorParameterViewModel(ParameterItem parameterItem, IVehicle vehicle) : this()
    {
        _vehicle = vehicle;
        
        Write = ReactiveCommand.CreateFromTask(WriteImpl);
        
        Update = ReactiveCommand.CreateFromTask(UpdateImpl);
        
        Parameter = parameterItem;

        this.WhenValueChanged(_ => _.Parameter)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => parameterItem = _);
        
        if (Parameter.Parameter.IntegerValue != null)
        {
            Value = (object)parameterItem.Parameter.IntegerValue;
            
            this.WhenValueChanged(_ => _.Value)
                .Subscribe(_ => Parameter.Parameter.IntegerValue = Convert.ToInt64(_))
                .DisposeItWith(Disposable);
        }
        
        if (Parameter.Parameter.RealValue != null)
        {
            Value = (object)parameterItem.Parameter.RealValue;
            
            this.WhenValueChanged(_ => _.Value)
                .Subscribe(_ => Parameter.Parameter.RealValue = Convert.ToSingle(_))
                .DisposeItWith(Disposable);
        }

        StringBuilder builder = new StringBuilder();

        if (Parameter.Description.Min != null & Parameter.Description.Max != null)
        {
            builder.Append($"{Parameter.Description.Min} - {Parameter.Description.Max}");
        }
        
        if (Parameter.Description.Increment != null)
        {
            builder.Append($" by {Parameter.Description.Increment}");
        }
        
        if (Parameter.Description.UnitsDisplayName != null)
        {
            builder.Append($" {Parameter.Description.UnitsDisplayName}");
        }

        ValueDescription = builder.ToString();
    }

    private async Task WriteImpl(CancellationToken cancel)
    {
        await _vehicle.Params.WriteParam(Parameter.Parameter, 3, cancel);
    }

    private async Task UpdateImpl(CancellationToken cancel)
    {
       Parameter.Parameter = await _vehicle.Params.ReadParam((short)Parameter.Parameter.Index, 3, cancel);
       
       if (Parameter.Parameter.IntegerValue != null)
       {
           Value = Parameter.Parameter.IntegerValue.Value;
       }
        
       if (Parameter.Parameter.RealValue != null)
       {
           Value = Parameter.Parameter.RealValue.Value;
       }
    }

    [Reactive]
    public object Value { get; set; }

    [Reactive]
    public string ValueDescription { get; set; }

    [Reactive]
    public ParameterItem Parameter { get; set; }
    
    [Reactive]
    public ICommand Write { get; set; }
    
    [Reactive]
    public ICommand Update { get; set; }

}