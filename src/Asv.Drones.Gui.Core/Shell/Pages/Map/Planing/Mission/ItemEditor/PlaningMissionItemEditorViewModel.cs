using System.ComponentModel.Composition;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Common;
using Avalonia.Controls;
using DocumentFormat.OpenXml.InkML;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Core;

[Export]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class PlaningMissionItemEditorViewModel : PlaningWidgetBase
{
    private readonly IPlaningMission _svc;
    private readonly ILocalizationService _loc;
    private IDisposable _selectedPointSubscription;

    public PlaningMissionItemEditorViewModel() : base(new Uri("asv:shell.page.mission.item-editor"))
    {
        if (Design.IsDesignMode)
        {
            
        }
    }

    [ImportingConstructor]
    public PlaningMissionItemEditorViewModel(IPlaningMission svc, ILocalizationService loc) : this()
    {
        _svc = svc;
        _loc = loc;
        
        Location = WidgetLocation.Right;
    }

    [Reactive]
    public string Param1 { get; set; }
    
    [Reactive]
    public string Param2 { get; set; }
    
    [Reactive]
    public string Param3 { get; set; }
    
    [Reactive]
    public string Param4 { get; set; }
    
    protected override void InternalAfterMapInit(IMap context)
    {
        Context = (IPlaningMissionContext)context;

        Context.WhenAnyValue(_ => _.Mission)
            .Subscribe(mission =>
            {
                _selectedPointSubscription?.Dispose();
                if(mission == null) return;
                _selectedPointSubscription = mission.WhenAnyValue(_ => _.SelectedPoint)
                    .Subscribe(selectedPoint =>
                    {
                        if(selectedPoint == null) return;

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
                if(Context.Mission is { SelectedPoint: not null })
                    Context.Mission.SelectedPoint.Point.Param1 = result;
            }).DisposeItWith(Disposable);
        this.WhenAnyValue(_ => _.Param2)
            .Subscribe(_ =>
            {
                if (!float.TryParse(_, CultureInfo.InvariantCulture, out var result)) return;
                if(Context.Mission is { SelectedPoint: not null })
                    Context.Mission.SelectedPoint.Point.Param2 = result;
            }).DisposeItWith(Disposable);
        this.WhenAnyValue(_ => _.Param3)
            .Subscribe(_ =>
            {
                if (!float.TryParse(_, CultureInfo.InvariantCulture, out var result)) return;
                if(Context.Mission is { SelectedPoint: not null })
                    Context.Mission.SelectedPoint.Point.Param3 = result;
            }).DisposeItWith(Disposable);
        this.WhenAnyValue(_ => _.Param4)
            .Subscribe(_ =>
            {
                if (!float.TryParse(_, CultureInfo.InvariantCulture, out var result)) return;
                if(Context.Mission is { SelectedPoint: not null })
                    Context.Mission.SelectedPoint.Point.Param4 = result;
            }).DisposeItWith(Disposable);
        
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
    
    [Reactive]
    public IPlaningMissionContext Context { get; set; }
}