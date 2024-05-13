using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class MeasureViewModel : TreePageWithValidationViewModel
{
    private readonly ILocalizationService _localization;

    public MeasureViewModel() : base(WellKnownUri.ShellPageSettingsMeasureUri)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MeasureViewModel(ISettingsPageContext context, ILocalizationService localization) : base(WellKnownUri
        .ShellPageSettingsAppearanceUri)
    {
        _localization = localization;

        _localization.Altitude.CurrentUnit.Subscribe(_ => SelectedAltitudeUnit = _)
            .DisposeItWith(Disposable);
        this.WhenValueChanged(_ => _.SelectedAltitudeUnit, false)
            .Subscribe(_localization.Altitude.CurrentUnit)
            .DisposeItWith(Disposable);

        _localization.Distance.CurrentUnit.Subscribe(_ => SelectedDistanceUnit = _)
            .DisposeItWith(Disposable);
        this.WhenValueChanged(_ => _.SelectedDistanceUnit, false)
            .Subscribe(_localization.Distance.CurrentUnit)
            .DisposeItWith(Disposable);

        _localization.Latitude.CurrentUnit.Subscribe(_ => SelectedLatitudeUnit = _)
            .DisposeItWith(Disposable);
        this.WhenValueChanged(_ => _.SelectedLatitudeUnit, false)
            .Subscribe(_localization.Latitude.CurrentUnit)
            .DisposeItWith(Disposable);

        _localization.Longitude.CurrentUnit.Subscribe(_ => SelectedLongitudeUnit = _)
            .DisposeItWith(Disposable);
        this.WhenValueChanged(_ => _.SelectedLongitudeUnit, false)
            .Subscribe(_localization.Longitude.CurrentUnit)
            .DisposeItWith(Disposable);

        _localization.Velocity.CurrentUnit.Subscribe(_ => SelectedVelocityUnit = _)
            .DisposeItWith(Disposable);
        this.WhenValueChanged(_ => _.SelectedVelocityUnit, false)
            .Subscribe(_localization.Velocity.CurrentUnit)
            .DisposeItWith(Disposable);

        _localization.Temperature.CurrentUnit.Subscribe(_ => SelectedTemperatureUnit = _)
            .DisposeItWith(Disposable);
        this.WhenValueChanged(_ => _.SelectedTemperatureUnit, false)
            .Subscribe(_localization.Temperature.CurrentUnit)
            .DisposeItWith(Disposable);

        _localization.Degree.CurrentUnit.Subscribe(_ => SelectedAngleUnit = _)
            .DisposeItWith(Disposable);
        this.WhenValueChanged(_ => _.SelectedAngleUnit, false)
            .Subscribe(_localization.Degree.CurrentUnit)
            .DisposeItWith(Disposable);
    }

    [Reactive] public IMeasureUnitItem<double, AltitudeUnits> SelectedAltitudeUnit { get; set; }

    public IEnumerable<IMeasureUnitItem<double, AltitudeUnits>> AltitudeUnits => _localization.Altitude.AvailableUnits;

    [Reactive] public IMeasureUnitItem<double, DistanceUnits> SelectedDistanceUnit { get; set; }

    public IEnumerable<IMeasureUnitItem<double, DistanceUnits>> DistanceUnits => _localization.Distance.AvailableUnits;

    [Reactive] public IMeasureUnitItem<double, LatitudeUnits> SelectedLatitudeUnit { get; set; }
    public IEnumerable<IMeasureUnitItem<double, LatitudeUnits>> LatitudeUnits => _localization.Latitude.AvailableUnits;

    [Reactive] public IMeasureUnitItem<double, LongitudeUnits> SelectedLongitudeUnit { get; set; }

    public IEnumerable<IMeasureUnitItem<double, LongitudeUnits>> LongitudeUnits =>
        _localization.Longitude.AvailableUnits;

    [Reactive] public IMeasureUnitItem<double, VelocityUnits> SelectedVelocityUnit { get; set; }

    public IEnumerable<IMeasureUnitItem<double, VelocityUnits>> VelocityUnits => _localization.Velocity.AvailableUnits;

    [Reactive] public IMeasureUnitItem<double, TemperatureUnits> SelectedTemperatureUnit { get; set; }

    public IEnumerable<IMeasureUnitItem<double, TemperatureUnits>> TemperatureUnits =>
        _localization.Temperature.AvailableUnits;

    public string TemperatureIcon => MaterialIconDataProvider.GetData(MaterialIconKind.Temperature);

    [Reactive] public IMeasureUnitItem<double, DegreeUnits> SelectedAngleUnit { get; set; }

    public IEnumerable<IMeasureUnitItem<double, DegreeUnits>> AngleUnits =>
        _localization.Degree.AvailableUnits;

    public string AngleIcon => MaterialIconDataProvider.GetData(MaterialIconKind.AngleAcute);
}