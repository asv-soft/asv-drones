
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Cfg.Json;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Uav;


[Export]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class TakeOffViewModel : ViewModelBaseWithValidation
{
    private ITakeOffService _service;
    [ImportingConstructor]
    public TakeOffViewModel(ITakeOffService service) : this() 
    {
        _service = service;

        _service?.CurrentAltitude.Subscribe(_ => Altitude = _).DisposeItWith(Disposable);
        this.WhenAnyValue(_ => _.Altitude)
            .Subscribe(_service.CurrentAltitude)
            .DisposeItWith(Disposable);

        if (Altitude == null)
        {
            Altitude = 30;
        }
        
        this.ValidationRule(x => x.Altitude, _ => _ >= 1, RS.SerialPortViewModel_SerialPortViewModel_You_must_specify_a_valid_name)
            .DisposeItWith(Disposable);
    }

    public TakeOffViewModel() : base(new Uri(UavAnchor.BaseUri, "actions.takeoff"))
    {
        
    }
    
    public void ApplyDialog(ContentDialog dialog)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));
        
        dialog.PrimaryButtonCommand =
            ReactiveCommand.Create(() => { }, this.IsValid().Do(_ =>dialog.IsPrimaryButtonEnabled = _)).DisposeItWith(Disposable);
    }

    [Reactive]
    public double? Altitude { get; set; }
}