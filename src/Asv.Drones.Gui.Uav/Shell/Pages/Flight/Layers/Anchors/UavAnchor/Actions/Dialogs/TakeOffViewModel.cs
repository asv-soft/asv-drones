
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Uav;


[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.Shared)]
public class TakeOffViewModel : ViewModelBaseWithValidation
{
    
    public const string UriString = UavAnchor.BaseUriString + ".actions.takeoff";
    public static readonly Uri Uri = new(UriString);


    [ImportingConstructor]
    public TakeOffViewModel(ITakeOffService service) : this()
    {
        service?.CurrentAltitude.Subscribe(_ => Altitude = _).DisposeItWith(Disposable);
        this.WhenAnyValue(_ => _.Altitude)
            .Subscribe(service.CurrentAltitude)
            .DisposeItWith(Disposable);

        this.ValidationRule(x => x.Altitude, _ => _ >= 1, RS.TakeOffAnchorActionViewModel_ValidValue)
            .DisposeItWith(Disposable);
    }

    public TakeOffViewModel() : base(Uri)
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