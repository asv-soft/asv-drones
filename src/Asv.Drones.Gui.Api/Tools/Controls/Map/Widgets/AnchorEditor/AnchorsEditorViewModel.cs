using System.Globalization;
using System.Reactive;
using Asv.Avalonia.Map;
using Asv.Common;
using Avalonia.Controls;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Api;

public class AnchorsEditorViewModel : MapWidgetBase
{
    private readonly ILocalizationService _loc;
    private bool _internalChange;
    private IDisposable? _locationSubscription;
    private IMapAnchor? _prevAnchor;
    #region ifDEBUG
    public AnchorsEditorViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
     if (Design.IsDesignMode)
        {
            IsVisible = true;
            IsEditable = true;
            IsCompactMode = true;
            Actions = new[]
            {
                new MapAnchorActionViewModel()
                {
                    Icon = MaterialIconKind.Accelerometer,
                    Title = "TakeOff"
                },
                new MapAnchorActionViewModel()
                {
                    Icon = MaterialIconKind.Accelerometer,
                    Title = "ROI"
                },
                new MapAnchorActionViewModel()
                {
                    Icon = MaterialIconKind.Accelerometer,
                    Title = "Land"
                },
                new MapAnchorActionViewModel()
                {
                    Icon = MaterialIconKind.Accelerometer,
                    Title = "Reboot"
                },
                new MapAnchorActionViewModel()
                {
                    Icon = MaterialIconKind.Accelerometer,
                    Title = "Start Mission"
                },
                new MapAnchorActionViewModel()
                {
                    Icon = MaterialIconKind.Accelerometer,
                    Title = ""
                },
                new MapAnchorActionViewModel()
                {
                    Icon = MaterialIconKind.Accelerometer,
                    Title = "Action"
                },
                new MapAnchorActionViewModel()
                {
                    Icon = MaterialIconKind.Accelerometer,
                    Title = ":Some"
                },
                new MapAnchorActionViewModel()
                {
                    Icon = MaterialIconKind.Accelerometer,
                    Title = ":Action"
                },
                new MapAnchorActionViewModel()
                {
                    Icon = MaterialIconKind.Accelerometer,
                    Title = "SuperAction"
                },
            };
        }
    }
    #endregion

    protected AnchorsEditorViewModel(Uri id, ILocalizationService loc) : base(id)
    {
        _loc = loc;
        Title = RS.AnchorsEditorViewModel_Title;
        Icon = MaterialIconKind.MapMarker;
        Location = WidgetLocation.Bottom;
        Disposable.AddAction(() => { _locationSubscription?.Dispose(); });
        Order = 0;

        CopyCommand = ReactiveCommand.Create(CopyGeoPoint).DisposeItWith(Disposable);
        PasteCommand = ReactiveCommand.Create(PasteGeoPoint).DisposeItWith(Disposable);

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

    protected AnchorsEditorViewModel(string id, ILocalizationService loc) : this(new Uri(id), loc)
    {
    }
    [Reactive] public bool IsTitleCompactMode { get; set; }
    [Reactive] public bool IsCompactMode { get; set; }

    [Reactive] public bool IsVisible { get; set; }

    [Reactive] public string Latitude { get; set; }

    [Reactive] public string LatitudeUnits { get; set; }

    [Reactive] public string Longitude { get; set; }

    [Reactive] public string LongitudeUnits { get; set; }

    [Reactive] public string Altitude { get; set; }

    [Reactive] public string AltitudeUnits { get; set; }

    [Reactive] public bool IsEditable { get; set; }

    [Reactive] public ReactiveCommand<Unit, Unit> CopyCommand { get; set; }
    [Reactive] public ReactiveCommand<Unit, Unit> PasteCommand { get; set; }
    
    [Reactive] private GeoPoint CopiedPoint { get; set; }
    [Reactive] public IEnumerable<MapAnchorActionViewModel> Actions { get; set; }



    private void CopyGeoPoint()
    {
        CopiedPoint = new GeoPoint(_loc.Latitude.ConvertToSi(Latitude), _loc.Longitude.ConvertToSi(Longitude),
            _loc.Altitude.ConvertToSi(Altitude));
    }

    private void PasteGeoPoint()
    {
        Latitude = CopiedPoint.Latitude.ToString(CultureInfo.InvariantCulture);
        Longitude = CopiedPoint.Longitude.ToString(CultureInfo.InvariantCulture);
        Altitude = CopiedPoint.Altitude.ToString(CultureInfo.InvariantCulture);
    }
    

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
                Altitude = _loc.Altitude.FromSiToString(prevLocation.Altitude);
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
                Actions = null;
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
                    if (_.Actions is not null) Actions = _.Actions.OrderByDescending(_=>_.Title.Length);
                    _prevAnchor = _;
                    IsVisible = true;
                    IsEditable = _.IsEditable;
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