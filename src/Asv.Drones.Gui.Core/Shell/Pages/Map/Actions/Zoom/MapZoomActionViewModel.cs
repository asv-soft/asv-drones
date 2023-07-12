using System.ComponentModel.Composition;
using System.Windows.Input;
using Asv.Common;
using Avalonia.Controls;
using ReactiveUI;

namespace Asv.Drones.Gui.Core;

public enum MapZoomValue
{
    Increase,
    Decrease
}

[Export(FlightPageViewModel.UriString,typeof(IMapAction))]
[Export(PlaningPageViewModel.UriString,typeof(IMapAction))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class MapZoomActionViewModel:ViewModelBase, IMapAction
{
  

    public MapZoomActionViewModel() : base("asv:shell.page.map.action.zoom")
    {
        
    }
    
    public IMap Map { get; private set;}
    public int Order { get; }
    public Dock Dock { get; }
    public IMapAction Init(IMap context)
    {
        Map = context;
        ZoomIn = ReactiveCommand.Create(() => ChangeZoomValue(MapZoomValue.Increase)).DisposeItWith(Disposable);
        ZoomOut = ReactiveCommand.Create(() => ChangeZoomValue(MapZoomValue.Decrease)).DisposeItWith(Disposable);
        return this;
    }

    private void ChangeZoomValue(MapZoomValue value)
    {
        if (value == MapZoomValue.Increase & Map.Zoom < Map.MaxZoom)
        {
            Map.Zoom++;
        }

        if (value == MapZoomValue.Decrease & Map.Zoom > Map.MinZoom)
        {
            Map.Zoom--;
        }
    }

    public ICommand ZoomIn { get; private set; }
    public ICommand ZoomOut { get; private set; }
    
}