using System;
using System.Collections.ObjectModel;
using System.Composition;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class HeaderPlaningFileMenuItem : MenuItem
{
    public const string UriString = $"{WellKnownUri.ShellPageMapPlaning}" + "/planing-file";
    public static readonly Uri Uri = new(UriString);
    private readonly ReadOnlyObservableCollection<IMenuItem> _items;

    [ImportingConstructor]
    public HeaderPlaningFileMenuItem() : base(Uri)
    {
        Header = RS.HeaderPlaningFileMenuItem_Title;
        Order = 0;
    }

    public override ReadOnlyObservableCollection<IMenuItem>? Items { get; set; }
}