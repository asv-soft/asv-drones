using System;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Avalonia.Media;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class PlaningMissionAnchor : MapAnchorBase
{
    public const string UriString = WellKnownUri.ShellPageMapPlaning + "/layer/{0}";

    protected PlaningMissionAnchor(PlaningMissionPointModel point) : base(
        new Uri(UriString.FormatWith($"{point.Index}/{point.Type}")))
    {
        Title = $"{point.Type.GetPlaningMissionPointName()} {point.Index}";
        Location = point.Location;
        IsFilled = true;
        IsEditable = true;
        IsVisible = true;
        OffsetX = OffsetXEnum.Center;
        OffsetY = OffsetYEnum.Center;
        PathOpacity = 0.6;
        IconBrush = Brushes.Purple;
    }

    [Reactive] public int Index { get; set; }
}