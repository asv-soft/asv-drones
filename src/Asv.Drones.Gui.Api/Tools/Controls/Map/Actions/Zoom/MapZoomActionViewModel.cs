using System.Windows.Input;
using Asv.Common;
using Avalonia.Controls;
using ReactiveUI;

namespace Asv.Drones.Gui.Api;

public class MapZoomActionViewModel : MapActionBase
{
    public MapZoomActionViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
        ZoomIn = ReactiveCommand.Create(Increase).DisposeItWith(Disposable);
        ZoomOut = ReactiveCommand.Create(Decrease).DisposeItWith(Disposable);
    }

    protected MapZoomActionViewModel(Uri id) : base(id)
    {
        ZoomIn = ReactiveCommand.Create(Increase).DisposeItWith(Disposable);
        ZoomOut = ReactiveCommand.Create(Decrease).DisposeItWith(Disposable);
    }

    protected MapZoomActionViewModel(string id) : this(new Uri(id))
    {
    }

    public override IMapAction Init(IMap context)
    {
        Dock = Dock.Left;
        base.Init(context);

        return this;
    }

    private void Decrease()
    {
        if (Map != null && Map.Zoom > Map.MinZoom)
        {
            Map.Zoom--;
        }
    }

    private void Increase()
    {
        if (Map != null && Map.Zoom < Map.MaxZoom)
        {
            Map.Zoom++;
        }
    }

    public ICommand ZoomIn { get; }
    public ICommand ZoomOut { get; }
}