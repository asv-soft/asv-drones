using System;
using Avalonia;

namespace Asv.Drones;

public partial class PitchItem : AvaloniaObject
{
    private readonly int _pitch;
    private string _title;
    private double _value;
    private Point _startLine;
    private Point _stopLine;
    private bool _isVisible;

    public PitchItem(
        int pitch,
        double scale,
        bool titleIsVisible = true,
        double controlHeight = 284
    )
    {
        _pitch = pitch;
        Value = ((controlHeight / 2) - pitch) * scale;
        if (titleIsVisible)
        {
            Title = pitch.ToString();
            StartLine = new Point(0 * scale, 0 * scale);
            StopLine = new Point(20 * scale, 0 * scale);
        }
        else
        {
            Title = string.Empty;
            StartLine = new Point(4 * scale, 0 * scale);
            StopLine = new Point(16 * scale, 0 * scale);
        }

        IsVisible = Math.Abs(pitch) <= 20;
    }

    public void UpdateVisibility(double pitch)
    {
        IsVisible = pitch >= _pitch - 20 && pitch <= _pitch + 20;
    }
}
