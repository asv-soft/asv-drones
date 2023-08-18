using System.ComponentModel.Composition;
using Asv.Common;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Core;

public class AnchorsEditorViewModel : MapWidgetBase
{
    private readonly ILocalizationService _loc;
    private bool _internalChange;
    private IDisposable? _locationSubscription;
    
    public const string UriString = "asv:shell.page.map.anchors-editor";

    public AnchorsEditorViewModel() : base(new Uri(UriString))
    {
    }

    [ImportingConstructor]
    public AnchorsEditorViewModel(ILocalizationService loc) : this()
    {
        _loc = loc;

        Disposable.AddAction(() =>
        {
            _locationSubscription?.Dispose();
        });
    }
    
    [Reactive]
    public bool IsVisible { get; set; }
    
    [Reactive]
    public string Latitude { get; set; }
    
    [Reactive]
    public string LatitudeUnits { get; set; }
    
    [Reactive]
    public string Longitude { get; set; }
    
    [Reactive]
    public string LongitudeUnits { get; set; }

    [Reactive]
    public string Altitude { get; set; }
    
    [Reactive]
    public string AltitudeUnits { get; set; }

    
    
    protected override void InternalAfterMapInit(IMap context)
    {
        LatitudeUnits = _loc.Latitude.CurrentUnit.Value.Unit;
        LongitudeUnits = _loc.Longitude.CurrentUnit.Value.Unit;
        AltitudeUnits = _loc.Altitude.CurrentUnit.Value.Unit;
        
        context.WhenValueChanged(_ => _.SelectedItem)
            .Subscribe(_ =>
            {
                _locationSubscription?.Dispose();
                _locationSubscription = null;
                
                if (_ != null)
                {
                    IsVisible = true;
                    
                    _locationSubscription = _.WhenAnyValue(_ => _.Location)
                        .Subscribe(_ =>
                        {
                            _internalChange = true;
                            
                            Latitude = _loc.Latitude.FromSiToString(_.Latitude);
                            Longitude = _loc.Longitude.FromSiToString(_.Longitude);
                            Altitude = _loc.Altitude.FromSiToString(_.Altitude);
                            
                            _internalChange = false;
                        });
                }
                else
                {
                    IsVisible = false;
                }
            })
            .DisposeItWith(Disposable);
        
        this.WhenPropertyChanged(_ => _.Latitude, false)
            .Subscribe(_ =>
            {
                if (_internalChange) return;

                if (context.SelectedItem != null && !string.IsNullOrWhiteSpace(_.Value) && 
                    _loc.Latitude.IsValid(_.Value) && context.SelectedItem.IsInEditMode)
                {
                    var prevLocation = context.SelectedItem.Location;
                    context.SelectedItem.Location = new GeoPoint(_loc.Latitude.CurrentUnit.Value.ConvertToSi(_.Value),
                        prevLocation.Longitude, prevLocation.Altitude);
                }
            })
            .DisposeItWith(Disposable);
        
        this.WhenPropertyChanged(_ => _.Longitude, false)
            .Subscribe(_ =>
            {
                if (_internalChange) return;
                
                if (context.SelectedItem != null && !string.IsNullOrWhiteSpace(_.Value)&& 
                    _loc.Longitude.IsValid(_.Value) && context.SelectedItem.IsInEditMode)
                {
                    var prevLocation = context.SelectedItem.Location;
                    context.SelectedItem.Location = new GeoPoint(prevLocation.Latitude,
                        _loc.Longitude.CurrentUnit.Value.ConvertToSi(_.Value), prevLocation.Altitude);
                }
            })
            .DisposeItWith(Disposable);
        
        this.WhenPropertyChanged(_ => _.Altitude, false)
            .Subscribe(_ =>
            {
                if (_internalChange) return;
                
                if (context.SelectedItem != null && !string.IsNullOrWhiteSpace(_.Value) && 
                    _loc.Altitude.IsValid(_.Value) && context.SelectedItem.IsInEditMode)
                {
                    var prevLocation = context.SelectedItem.Location;
                    context.SelectedItem.Location = new GeoPoint(prevLocation.Latitude,
                        prevLocation.Longitude, _loc.Altitude.CurrentUnit.Value.ConvertToSi(_.Value));
                }
            })
            .DisposeItWith(Disposable);
        
        this.ValidationRule(x => x.Latitude,
                _ => _loc.Latitude.IsValid(_),
                _ => _loc.Latitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);
        
        this.ValidationRule(x => x.Longitude,
                _ => _loc.Longitude.IsValid(_),
                _ => _loc.Longitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);
        
        this.ValidationRule(x => x.Altitude,
                _ => _loc.Altitude.IsValid(_),
                _ => _loc.Altitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);
    }
}