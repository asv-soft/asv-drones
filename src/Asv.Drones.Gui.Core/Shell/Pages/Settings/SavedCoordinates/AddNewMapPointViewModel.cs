using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Core;

public class AddNewMapPointViewModel : ViewModelBaseWithValidation
{
    private readonly IConfiguration _cfg;
    private readonly ILocalizationService _loc;
    
    private const double MinimumAccuracyDistance = 0.01;
    private const int MinimumLatitudeValue = -90;
    private const int MaximumLatitudeValue = 90;
    private const int MinimumLongitudeValue = -180;
    private const int MaximumLongitudeValue = 180;

    public AddNewMapPointViewModel() : base(new Uri(SavedCoordsViewModel.Uri, "addnew"))
    {
    }
    
    [ImportingConstructor]
    public AddNewMapPointViewModel(FixedModeConfig cfg, ILocalizationService loc, IConfiguration configuration) : this()
    {
        _cfg = configuration;
        _loc = loc;
        Latitude = loc.Latitude.FromSiToString(cfg.Latitude);
        Longitude = loc.Longitude.FromSiToString(cfg.Longitude);
        Altitude = loc.Altitude.FromSiToString(cfg.Altitude);
        Accuracy = loc.Distance.FromSiToString(cfg.Accuracy);
        Name = cfg.Name;
        
        #region Validation Rules

        this.ValidationRule(x => x.Name,
                _ => !string.IsNullOrWhiteSpace(_),
                RS.AddNewMapPointViewModel_Name_ValidValue)
            .DisposeItWith(Disposable);

        this.ValidationRule(x => x.Accuracy,
                _ => _loc.Distance.IsValid(MinimumAccuracyDistance, double.MaxValue, _),
                string.Format(RS.AddNewMapPointViewModel_Accuracy_ValidValue,
                    _loc.Distance.FromSiToString(MinimumAccuracyDistance)))
            .DisposeItWith(Disposable);

        this.ValidationRule(x => x.Latitude, _ => _loc.Latitude.IsValid(MinimumLatitudeValue, MaximumLatitudeValue, _),
                _ => _loc.Latitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);
        
        
        this.ValidationRule(x => x.Longitude, _ => _loc.Longitude.IsValid(MinimumLongitudeValue, MaximumLongitudeValue, _),
                _ => _loc.Longitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);
      

        this.ValidationRule(x => x.Altitude, _ => _loc.Altitude.IsValid(_),
                _ => _loc.Altitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);
        
        #endregion
    }

    public void ApplyDialog(ContentDialog dialog)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));

        dialog.PrimaryButtonCommand = ReactiveCommand
            .Create(AddItem, this.IsValid().Do(_ => dialog.IsPrimaryButtonEnabled = _))
            .DisposeItWith(Disposable);
    }

    private void AddItem()
    {
        var coords = _cfg.Get<FixedModeSavedCoords>();
        
        coords.Coords.Add(new FixedModeConfig
        {
            Latitude = _loc.Latitude.ConvertToSi(Latitude),
            Longitude = _loc.Longitude.ConvertToSi(Longitude),
            Altitude = _loc.Altitude.ConvertToSi(Altitude),
            Accuracy = _loc.Distance.ConvertToSi(Accuracy),
            Name = Name
        });
        
        _cfg.Set(coords);
    }
    
    [Reactive]
    public string Latitude { get; set; } = "0";
    [Reactive]
    public string Longitude { get; set; } = "0";
    [Reactive]
    public string Altitude { get; set; } = "0";
    [Reactive]
    public string Accuracy { get; set; } = "0";
    [Reactive]
    public string Name { get; set; }
    public string AccuracyUnits => _loc.Distance.CurrentUnit.Value.Unit;
    public string LatitudeUnits => _loc.Latitude.CurrentUnit.Value.Unit;
    public string LongitudeUnits => _loc.Longitude.CurrentUnit.Value.Unit;
    public string AltitudeUnits => _loc.Altitude.CurrentUnit.Value.Unit;
}