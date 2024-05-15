using System;
using System.Composition;
using System.Globalization;
using Asv.Common;
using Asv.Drones.Gui.Api;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapPlaning, typeof(IMapWidget))]
public class PlaningMissionItemEditorViewModel : MapWidgetBase
{
    private readonly IPlaningMission _svc;
    private readonly ILocalizationService _loc;
    private IDisposable _selectedPointSubscription;

    public PlaningMissionItemEditorViewModel() : base(WellKnownUri.Undefined)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public PlaningMissionItemEditorViewModel(IPlaningMission svc, ILocalizationService loc) : base(WellKnownUri
        .ShellPageMapPlaningWidgetItemEditor)
    {
        _svc = svc;
        _loc = loc;
        Order = 0;
        Location = WidgetLocation.Right;
    }

    [Reactive] public string Param1 { get; set; }

    [Reactive] public string Param2 { get; set; }

    [Reactive] public string Param3 { get; set; }

    [Reactive] public string Param4 { get; set; }

    protected override void InternalAfterMapInit(IMap context)
    {
        Context = (IPlaningMissionContext)context;

        Context.WhenAnyValue(_ => _.Mission)
            .Subscribe(mission =>
            {
                _selectedPointSubscription?.Dispose();
                if (mission == null) return;
                _selectedPointSubscription = mission.WhenAnyValue(_ => _.SelectedPoint)
                    .Subscribe(selectedPoint =>
                    {
                        if (selectedPoint == null) return;

                        Param1 = selectedPoint.Point.Param1.ToString(CultureInfo.InvariantCulture);
                        Param2 = selectedPoint.Point.Param2.ToString(CultureInfo.InvariantCulture);
                        Param3 = selectedPoint.Point.Param3.ToString(CultureInfo.InvariantCulture);
                        Param4 = selectedPoint.Point.Param4.ToString(CultureInfo.InvariantCulture);
                    });
            })
            .DisposeItWith(Disposable);

        this.WhenAnyValue(_ => _.Param1)
            .Subscribe(_ =>
            {
                if (!float.TryParse(_, CultureInfo.InvariantCulture, out var result)) return;
                if (Context.Mission is { SelectedPoint: not null })
                    Context.Mission.SelectedPoint.Point.Param1 = result;
            }).DisposeItWith(Disposable);
        this.WhenAnyValue(_ => _.Param2)
            .Subscribe(_ =>
            {
                if (!float.TryParse(_, CultureInfo.InvariantCulture, out var result)) return;
                if (Context.Mission is { SelectedPoint: not null })
                    Context.Mission.SelectedPoint.Point.Param2 = result;
            }).DisposeItWith(Disposable);
        this.WhenAnyValue(_ => _.Param3)
            .Subscribe(_ =>
            {
                if (!float.TryParse(_, CultureInfo.InvariantCulture, out var result)) return;
                if (Context.Mission is { SelectedPoint: not null })
                    Context.Mission.SelectedPoint.Point.Param3 = result;
            }).DisposeItWith(Disposable);
        this.WhenAnyValue(_ => _.Param4)
            .Subscribe(_ =>
            {
                if (!float.TryParse(_, CultureInfo.InvariantCulture, out var result)) return;
                if (Context.Mission is { SelectedPoint: not null })
                    Context.Mission.SelectedPoint.Point.Param4 = result;
            }).DisposeItWith(Disposable);

        // TODO: Localize
        this.ValidationRule(_ => _.Param1,
            value => float.TryParse(value, out _),
            "Param1 не может быть null").DisposeItWith(Disposable);
        this.ValidationRule(_ => _.Param2,
            value => float.TryParse(value, out _),
            "Param2 не может быть null").DisposeItWith(Disposable);
        this.ValidationRule(_ => _.Param3,
            value => float.TryParse(value, out _),
            "Param3 не может быть null").DisposeItWith(Disposable);
        this.ValidationRule(_ => _.Param4,
            value => float.TryParse(value, out _),
            "Param4 не может быть null").DisposeItWith(Disposable);
    }

    [Reactive] public IPlaningMissionContext Context { get; set; }
}