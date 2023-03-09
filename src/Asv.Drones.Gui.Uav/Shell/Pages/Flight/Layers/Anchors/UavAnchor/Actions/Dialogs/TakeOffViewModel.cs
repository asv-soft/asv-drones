
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Cfg;
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
    private readonly IConfiguration _cfg;
    private readonly ILocalizationService _loc;
    
    public const string UriString = UavAnchor.BaseUriString + ".actions.takeoff";
    public static readonly Uri Uri = new(UriString);


    [ImportingConstructor]
    public TakeOffViewModel(IConfiguration cfg, ILocalizationService loc) : this()
    {
        _cfg = cfg;
        _loc = loc;
        
        Altitude = _loc.Altitude.GetDoubleValue(_cfg.Get<double?>("TakeOffAltitude") ?? 30, false);

        this.ValidationRule(x => x.Altitude, 
                _ => _ >= _loc.Altitude.GetDoubleValue(1, false), 
                string.Format(RS.TakeOffAnchorActionViewModel_ValidValue, _loc.Altitude.GetValue(1)))
            .DisposeItWith(Disposable);
    }

    public TakeOffViewModel() : base(Uri)
    {
        
    }
    
    public void ApplyDialog(ContentDialog dialog)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));
        
        dialog.PrimaryButtonCommand =
            ReactiveCommand.Create(() => { _cfg.Set("TakeOffAltitude", _loc.Altitude.GetDoubleValue(Altitude.Value, true)); },
                this.IsValid().Do(_ =>dialog.IsPrimaryButtonEnabled = _)).DisposeItWith(Disposable);
    }

    [Reactive]
    public double? Altitude { get; set; }
    
    public string Units
    {
        get => _loc.Altitude.GetUnit(0);
    }
}