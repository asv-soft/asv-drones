using System;
using Asv.Drones.Gui.Api;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Drones.Gui;

public class HeaderPlaningFileDeleteMenuItem : MenuItem
{
    public const string UriString = HeaderPlaningFileMenuItem.UriString + "/delete";
    public static readonly Uri Uri = new(UriString);

    public HeaderPlaningFileDeleteMenuItem() : base(Uri)
    {
        Header = RS.HeaderPlaningFileDeleteMenuItem_Title;
        Icon = MaterialIconKind.Delete;
        HotKey = new KeyGesture(Key.D, KeyModifiers.Control);
        Order = ushort.MaxValue;
    }
}