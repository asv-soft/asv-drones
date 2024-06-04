using System;
using Asv.Drones.Gui.Api;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Drones.Gui;

public class HeaderPlaningFileSaveMenuItem : MenuItem
{
    public const string UriString = HeaderPlaningFileMenuItem.UriString + "/save";
    public static readonly Uri Uri = new(UriString);

    public HeaderPlaningFileSaveMenuItem() : base(Uri)
    {
        Header = RS.HeaderPlaningFileSaveMenuItem_Title;
        Icon = MaterialIconKind.ContentSave;
        HotKey = new KeyGesture(Key.S, KeyModifiers.Control);
        Order = ushort.MaxValue;
    }
}