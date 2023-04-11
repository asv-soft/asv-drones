using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
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
        
        if(!parameterItem.Description.IsReadOnly)
            Write = ReactiveCommand.CreateFromObservable(() => Observable.FromAsync(WriteImpl)
                .SubscribeOn(RxApp.MainThreadScheduler));
        
        Update = ReactiveCommand.CreateFromObservable(() => Observable.FromAsync(UpdateImpl)
            .SubscribeOn(RxApp.MainThreadScheduler));

        Parameter = parameterItem;
        
        this.WhenValueChanged(_ => _.Parameter)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => parameterItem = _);
        
        
        if (Parameter.Parameter.IntegerValue != null)
        {
            Value = parameterItem.Parameter.IntegerValue;
            
            this.WhenValueChanged(_ => _.Value)
                .Subscribe(_ =>
                {
                    if(long.TryParse(Value.ToString(), out var result))
                        Parameter.Parameter.IntegerValue = result;
                })
                .DisposeItWith(Disposable);
        }
        
        if (Parameter.Parameter.RealValue != null)
        {
            Value = parameterItem.Parameter.RealValue;

            this.WhenValueChanged(_ => _.Value)
                .Subscribe(_ =>
                {
                    if(float.TryParse(Value.ToString(), out var result))
                        Parameter.Parameter.RealValue = result;
                })
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
        if(float.TryParse(Value.ToString(), out var resultFloat) || long.TryParse(Value.ToString(), out var resultLong))
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
    public ReactiveCommand<Unit, Unit> Write { get; set; }
    
    [Reactive]
    public ReactiveCommand<Unit, Unit> Update { get; set; }

}