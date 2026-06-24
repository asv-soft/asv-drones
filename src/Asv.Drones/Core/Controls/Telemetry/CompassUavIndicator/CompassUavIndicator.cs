using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Primitives;

namespace Asv.Drones;

public partial class CompassUavIndicator : TemplatedControl
{
    public CompassUavIndicator()
    {
        CompassItems = new AvaloniaList<CompassScaleItem>(
            Enumerable
                .Range(0, 24)
                .Select(index =>
                {
                    var angle = index * 15.0;
                    var title = angle switch
                    {
                        0 => RS.HeadingScaleItem_Direction_N,
                        90 => RS.HeadingScaleItem_Direction_E,
                        180 => RS.HeadingScaleItem_Direction_S,
                        270 => RS.HeadingScaleItem_Direction_W,
                        _ when angle % 30 == 0 => angle.ToString("F0"),
                        _ => null,
                    };
                    return new CompassScaleItem(angle, title, angle % 30 == 0);
                })
        );
        UpdateCompass();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == HeadingProperty || change.Property == HomeAzimuthProperty)
        {
            UpdateCompass();
        }
    }

    private void UpdateCompass()
    {
        var heading = NormalizeAngle(Heading);
        HeadingText = $"{heading:F0}°";

        foreach (var item in CompassItems)
        {
            item.Update(heading);
        }

        HomeMarkerRotation = double.IsNaN(HomeAzimuth)
            ? 0.0
            : NormalizeSignedAngle(HomeAzimuth - heading);
    }

    private static double NormalizeAngle(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return 0.0;
        }

        var angle = value % 360.0;
        return angle < 0.0 ? angle + 360.0 : angle;
    }

    private static double NormalizeSignedAngle(double value)
    {
        var angle = value % 360.0;
        if (angle <= -180.0)
        {
            angle += 360.0;
        }
        else if (angle > 180.0)
        {
            angle -= 360.0;
        }

        return angle;
    }
}
