using System;
using Asv.Drones.Gui.Api;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Drones.Gui;

public class HeaderPlaningFileOpenMenuItem : MenuItem
{
    public const string UriString = HeaderPlaningFileMenuItem.UriString + "/open";
    public static readonly Uri Uri = new(UriString);

    public HeaderPlaningFileOpenMenuItem() : base(Uri)
    {
        Header = RS.HeaderPlaningFileOpenMenuItem_Title;
        Icon = MaterialIconKind.FolderOpen;
        HotKey = new KeyGesture(Key.O, KeyModifiers.Control);
        Order = ushort.MaxValue;
    }
}