using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Common;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

public class RemoveMapPointViewModel : ViewModelBase
{
    private readonly IConfiguration _cfg;
    private readonly ILocalizationService _loc;
    private readonly int _indexToRemove;
    
    public RemoveMapPointViewModel() : base(new Uri(SavedCoordsViewModel.Uri, "remove"))
    {
    }
    
    [ImportingConstructor]
    public RemoveMapPointViewModel(FixedModeConfig fixedModeConfig, int indexToRemove, ILocalizationService loc, IConfiguration configuration) : this()
    {
        _cfg = configuration;
        _loc = loc;
        _indexToRemove = indexToRemove;
        FixedModeConfig = fixedModeConfig;
    }

    public void ApplyDialog(ContentDialog dialog)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));

        dialog.PrimaryButtonCommand = ReactiveCommand.Create(RemoveItem).DisposeItWith(Disposable);
    }

    private void RemoveItem()
    {
        var coords = _cfg.Get<FixedModeSavedCoords>();

        coords.Coords.RemoveAt(_indexToRemove);
        
        _cfg.Set(coords);
    }
    
    [Reactive]
    public FixedModeConfig FixedModeConfig { get; set; }

    public string Accuracy => _loc.Distance.FromSiToString(FixedModeConfig.Accuracy);
    public string Altitude => _loc.Altitude.FromSiToString(FixedModeConfig.Altitude);
    public string Latitude => _loc.Latitude.FromSiToString(FixedModeConfig.Latitude);
    public string Longitude => _loc.Longitude.FromSiToString(FixedModeConfig.Longitude);
    
    public string AccuracyUnits => _loc.Distance.CurrentUnit.Value.Unit;
    public string LatitudeUnits => _loc.Latitude.CurrentUnit.Value.Unit;
    public string LongitudeUnits => _loc.Longitude.CurrentUnit.Value.Unit;
    public string AltitudeUnits => _loc.Altitude.CurrentUnit.Value.Unit;
}