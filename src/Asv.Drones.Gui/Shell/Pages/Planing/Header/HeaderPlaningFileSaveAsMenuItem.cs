using System;
using Asv.Drones.Gui.Api;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Drones.Gui;

public class HeaderPlaningFileSaveAsMenuItem : MenuItem
{
    public const string UriString = HeaderPlaningFileMenuItem.UriString + "/save_as";
    public static readonly Uri Uri = new(UriString);

    public HeaderPlaningFileSaveAsMenuItem() : base(Uri)
    {
        Header = RS.HeaderPlaningFileSaveAsMenuItem_Title;
        Icon = MaterialIconKind.ContentSave;
        HotKey = new KeyGesture(Key.W, KeyModifiers.Control);
        Order = ushort.MaxValue;
    }
}