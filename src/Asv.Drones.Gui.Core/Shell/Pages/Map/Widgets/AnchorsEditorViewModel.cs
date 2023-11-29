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
    private IMapAnchor _prevAnchor;
    
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

        Order = int.MaxValue;
        
        this.WhenAnyValue(_ => _.Latitude)
            .Subscribe(UpdateLatitude)
            .DisposeItWith(Disposable);
        this.WhenAnyValue(_ => _.Longitude)
            .Subscribe(UpdateLongitude)
            .DisposeItWith(Disposable);
        this.WhenAnyValue(_ => _.Altitude)
            .Subscribe(UpdateAltitude)
            .DisposeItWith(Disposable);
        
        this.ValidationRule(x => x.Latitude,
                _ => _loc.Latitude.IsValid(_),
                _ => _loc.Latitude.GetErrorMessage(_) ?? string.Empty)
            .DisposeItWith(Disposable);
        
        this.ValidationRule(x => x.Longitude,
                _ => _loc.Longitude.IsValid(_),
                _ => _loc.Longitude.GetErrorMessage(_) ?? string.Empty)
            .DisposeItWith(Disposable);
        
        this.ValidationRule(x => x.Altitude,
                _ => _loc.Altitude.IsValid(_),
                _ => _loc.Altitude.GetErrorMessage(_) ?? string.Empty)
            .DisposeItWith(Disposable);
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

    private void UpdateLatitude(string latitude)
    {
        if (_internalChange) return;
        if (!string.IsNullOrWhiteSpace(latitude) && _loc.Latitude.IsValid(latitude))
        {
            var prevLocation = _prevAnchor.Location;
            var newLocation = _prevAnchor.Location = new GeoPoint(
                _loc.Latitude.CurrentUnit.Value.ConvertToSi(latitude),
                prevLocation.Longitude, prevLocation.Altitude);    
            // if new location is equal to previous location then we need to update longitude string
            if (prevLocation.Equals(newLocation))
            {
                _internalChange = true;
                Latitude = _loc.Longitude.FromSiToString(prevLocation.Latitude);
                _internalChange = false;
            }
            else
            {
                _prevAnchor.Location = newLocation;
            }
        }
    }
    private void UpdateLongitude(string longitude)
    {
        if (_internalChange) return;
        if (!string.IsNullOrWhiteSpace(longitude) && _loc.Longitude.IsValid(longitude))
        {
            var prevLocation = _prevAnchor.Location;
            var newLocation = new GeoPoint(prevLocation.Latitude,
                _loc.Longitude.CurrentUnit.Value.ConvertToSi(longitude), 
                prevLocation.Altitude);
            // if new location is equal to previous location then we need to update longitude string
            if (prevLocation.Equals(newLocation))
            {
                _internalChange = true;
                Longitude = _loc.Longitude.FromSiToString(prevLocation.Longitude);
                _internalChange = false;
            }
            else
            {
                _prevAnchor.Location = newLocation;
            }
        }
    }
    private void UpdateAltitude(string altitude)
    {
        if (_internalChange) return;
        if (!string.IsNullOrWhiteSpace(altitude) && _loc.Altitude.IsValid(altitude))
        {
            var prevLocation = _prevAnchor.Location;
            var newLocation = _prevAnchor.Location = new GeoPoint(prevLocation.Latitude,
                prevLocation.Longitude, 
                _loc.Altitude.CurrentUnit.Value.ConvertToSi(altitude));
            // if new location is equal to previous location then we need to update string
            if (prevLocation.Equals(newLocation))
            {
                _internalChange = true;
                Altitude = _loc.Longitude.FromSiToString(prevLocation.Altitude);
                _internalChange = false;
            }
            else
            {
                _prevAnchor.Location = newLocation;
            }
        }
    }
    
    protected override void InternalAfterMapInit(IMap context)
    {
        LatitudeUnits = _loc.Latitude.CurrentUnit.Value.Unit;
        LongitudeUnits = _loc.Longitude.CurrentUnit.Value.Unit;
        AltitudeUnits = _loc.Altitude.CurrentUnit.Value.Unit;
        
        context.WhenValueChanged(_ => _.SelectedItem)
            .Subscribe(_ =>
            {
                if (_prevAnchor != null && _prevAnchor.IsInEditMode)
                {
                    UpdateLatitude(Latitude);
                    UpdateLongitude(Longitude);
                    UpdateAltitude(Altitude);
                }
                
                _locationSubscription?.Dispose();
                _locationSubscription = null;
                
                if (_ != null)
                {
                    _prevAnchor = _;
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
        
       
    }
}