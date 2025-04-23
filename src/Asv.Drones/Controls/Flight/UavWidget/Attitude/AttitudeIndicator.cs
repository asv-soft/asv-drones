using System;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using R3;

namespace Asv.Drones;

public partial class AttitudeIndicator : TemplatedControl
{
    private const int VelocityItemCount = 6;
    private const int VelocityValueRange = 5;
    private const double VelocityControlLengthPrc = 0.4;
    private const int AltitudeItemCount = 6;
    private const int AltitudeValueRange = 5;
    private const double AltitudeControlLengthPrc = 0.4;
    private const int HeadingItemCount = 10;
    private const double HeadingControlLengthPrc = 1.0;
    private const int HeadingValueRange = 15;

    private static double _headingPositionStep;
    private static double _headingCenterPosition;

    public double Scale { get; }

    public AttitudeIndicator()
    {
        if (Design.IsDesignMode)
        {
            var status = new[] { "Armed", "Disarmed" };
            Observable
                .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), TimeProvider.System)
                .Subscribe(_ =>
                {
                    StatusText = status[1 % 2];
                });
            StatusText = status[1];
        }

        var smallerSide = Math.Min(InternalWidth, InternalHeight);
        Scale = smallerSide / 100;

        RollItems = new AvaloniaList<RollItem>(
            new RollItem(0),
            new RollItem(10),
            new RollItem(20),
            new RollItem(30),
            new RollItem(45),
            new RollItem(60),
            new RollItem(300),
            new RollItem(315),
            new RollItem(330),
            new RollItem(340),
            new RollItem(350)
        );

        PitchItems = new AvaloniaList<PitchItem>(
            new PitchItem(135, Scale, false),
            new PitchItem(130, Scale),
            new PitchItem(125, Scale, false),
            new PitchItem(120, Scale),
            new PitchItem(115, Scale, false),
            new PitchItem(110, Scale),
            new PitchItem(105, Scale, false),
            new PitchItem(100, Scale),
            new PitchItem(95, Scale, false),
            new PitchItem(90, Scale),
            new PitchItem(85, Scale, false),
            new PitchItem(80, Scale),
            new PitchItem(75, Scale, false),
            new PitchItem(70, Scale),
            new PitchItem(65, Scale, false),
            new PitchItem(60, Scale),
            new PitchItem(55, Scale, false),
            new PitchItem(50, Scale),
            new PitchItem(45, Scale, false),
            new PitchItem(40, Scale),
            new PitchItem(35, Scale, false),
            new PitchItem(30, Scale),
            new PitchItem(25, Scale, false),
            new PitchItem(20, Scale),
            new PitchItem(15, Scale, false),
            new PitchItem(10, Scale),
            new PitchItem(5, Scale, false),
            new PitchItem(0, Scale),
            new PitchItem(-5, Scale, false),
            new PitchItem(-10, Scale),
            new PitchItem(-15, Scale, false),
            new PitchItem(-20, Scale),
            new PitchItem(-25, Scale, false),
            new PitchItem(-30, Scale),
            new PitchItem(-35, Scale, false),
            new PitchItem(-40, Scale),
            new PitchItem(-45, Scale, false),
            new PitchItem(-50, Scale),
            new PitchItem(-55, Scale, false),
            new PitchItem(-60, Scale),
            new PitchItem(-65, Scale, false),
            new PitchItem(-70, Scale),
            new PitchItem(-75, Scale, false),
            new PitchItem(-80, Scale),
            new PitchItem(-85, Scale, false),
            new PitchItem(-90, Scale),
            new PitchItem(-95, Scale, false),
            new PitchItem(-100, Scale),
            new PitchItem(-105, Scale, false),
            new PitchItem(-110, Scale),
            new PitchItem(-115, Scale, false),
            new PitchItem(-120, Scale),
            new PitchItem(-125, Scale, false),
            new PitchItem(-130, Scale),
            new PitchItem(-135, Scale, false)
        );

        var velocityControlLength = smallerSide * VelocityControlLengthPrc;
        var velocityItemLength = velocityControlLength / (VelocityItemCount - 1);
        VelocityItems = new AvaloniaList<ScaleItem>(
            Enumerable
                .Range(0, VelocityItemCount)
                .Select(_ => new ScaleItem(
                    0,
                    VelocityValueRange,
                    _,
                    VelocityItemCount,
                    velocityControlLength + velocityItemLength,
                    velocityControlLength,
                    showNegative: false
                ))
        );

        var altitudeControlLength = smallerSide * AltitudeControlLengthPrc;
        var altitudeItemLength = altitudeControlLength / (AltitudeItemCount - 1);
        AltitudeItems = new AvaloniaList<ScaleItem>(
            Enumerable
                .Range(0, AltitudeItemCount)
                .Select(_ => new ScaleItem(
                    0,
                    AltitudeValueRange,
                    _,
                    AltitudeItemCount,
                    altitudeControlLength + altitudeItemLength,
                    altitudeControlLength
                ))
        );

        var headingControlLength = smallerSide * HeadingControlLengthPrc;
        var headingItemLength = headingControlLength / (HeadingItemCount - 1);
        HeadingItems = new AvaloniaList<ScaleItem>(
            Enumerable
                .Range(0, HeadingItemCount)
                .Select(_ => new HeadingScaleItem(
                    0,
                    HeadingValueRange,
                    _,
                    HeadingItemCount,
                    headingControlLength + headingItemLength,
                    headingControlLength
                ))
        );

        var headingItemStep =
            (headingControlLength + headingItemLength)
            / (HeadingItemCount % 2 != 0 ? HeadingItemCount - 1 : HeadingItemCount);
        _headingPositionStep = -1 * headingItemStep / HeadingValueRange;
        _headingCenterPosition = headingControlLength / 2;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == VibrationXProperty)
        {
            UpdateColorX(change.Sender);
        }
        else if (change.Property == VibrationYProperty)
        {
            UpdateColorY(change.Sender);
        }
        else if (change.Property == VibrationZProperty)
        {
            UpdateColorZ(change.Sender);
        }
        else if (change.Property == VelocityProperty)
        {
            UpdateVelocityItems(change.Sender);
        }
        else if (change.Property == RollAngleProperty)
        {
            UpdateRollAngle(change.Sender);
        }
        else if (change.Property == PitchAngleProperty)
        {
            UpdateAngle(change.Sender);
        }
        else if (change.Property == AltitudeProperty)
        {
            UpdateAltitudeItems(change.Sender);
        }
        else if (change.Property == HeadingProperty)
        {
            UpdateHeadingItems(change.Sender);
        }
        else if (change.Property == HomeAzimuthProperty)
        {
            UpdateHomeAzimuthPosition(change.Sender);
        }
    }

    private static void UpdateColorX(AvaloniaObject source)
    {
        if (source is not AttitudeIndicator indicator)
        {
            return;
        }

        if (indicator.VibrationX < 30)
        {
            indicator.BrushVibrationX = Colors.Red;
        }
        else if (indicator.VibrationX is > 30 and < 60)
        {
            indicator.BrushVibrationX = Colors.Yellow;
        }
        else if (indicator.VibrationX > 60)
        {
            indicator.BrushVibrationX = Colors.GreenYellow;
        }
    }

    private static void UpdateColorY(AvaloniaObject source)
    {
        if (source is not AttitudeIndicator indicator)
        {
            return;
        }

        if (indicator.VibrationY < 30)
        {
            indicator.BrushVibrationY = Colors.Red;
        }
        else if (indicator.VibrationY > 30 & indicator.VibrationY < 60)
        {
            indicator.BrushVibrationY = Colors.Yellow;
        }
        else if (indicator.VibrationY > 60)
        {
            indicator.BrushVibrationY = Colors.GreenYellow;
        }
    }

    private static void UpdateColorZ(AvaloniaObject source)
    {
        if (source is not AttitudeIndicator indicator)
        {
            return;
        }

        if (indicator.VibrationZ < 30)
        {
            indicator.BrushVibrationZ = Colors.Red;
        }
        else if (indicator.VibrationZ > 30 & indicator.VibrationZ < 60)
        {
            indicator.BrushVibrationZ = Colors.Yellow;
        }
        else if (indicator.VibrationZ > 60)
        {
            indicator.BrushVibrationZ = Colors.GreenYellow;
        }
    }

    private static void UpdateAngle(AvaloniaObject source)
    {
        if (source is not AttitudeIndicator indicator)
        {
            return;
        }

        var pitch = indicator.PitchAngle;
        UpdateRollAngle(source);
        foreach (var item in indicator.PitchItems)
        {
            item.UpdateVisibility(pitch);
        }
    }

    private static void UpdateRollAngle(AvaloniaObject source)
    {
        if (source is not AttitudeIndicator indicator)
        {
            return;
        }

        var roll = indicator.RollAngle;
        var pitch = indicator.PitchAngle;
        indicator.PitchTranslateX =
            -pitch * indicator.Scale * Math.Cos((roll - 90.0) * Math.PI / 180.0);
        indicator.PitchTranslateY =
            pitch * indicator.Scale * Math.Sin((90 - roll) * Math.PI / 180.0);
    }

    private static void UpdateVelocityItems(AvaloniaObject source)
    {
        if (source is not AttitudeIndicator indicator)
        {
            return;
        }

        double velocity;
        if (indicator.Velocity is null)
        {
            velocity = double.NaN;
        }
        else
        {
            var isParsed = double.TryParse(indicator.Velocity, out velocity);
            velocity = !isParsed ? double.NaN : velocity;
        }

        foreach (var item in indicator.VelocityItems)
        {
            item.UpdateValue(velocity);
        }
    }

    private static void UpdateAltitudeItems(AvaloniaObject source)
    {
        if (source is not AttitudeIndicator indicator)
        {
            return;
        }

        double altitude;
        if (indicator.Altitude is null)
        {
            altitude = double.NaN;
        }
        else
        {
            var isParsed = double.TryParse(indicator.Altitude, out altitude);
            altitude = !isParsed ? double.NaN : altitude;
        }

        foreach (var item in indicator.AltitudeItems)
        {
            item.UpdateValue(altitude);
        }
    }

    private static void UpdateHeadingItems(AvaloniaObject source)
    {
        if (source is not AttitudeIndicator indicator)
        {
            return;
        }

        double heading;
        if (indicator.Heading is null)
        {
            heading = double.NaN;
        }
        else
        {
            var isParsed = double.TryParse(indicator.Heading, out heading);
            heading = !isParsed ? double.NaN : heading;
        }

        double.TryParse(indicator.HomeAzimuth, out var homeAzimuth);

        foreach (var item in indicator.HeadingItems)
        {
            item.UpdateValue(heading);
        }

        indicator.HomeAzimuthPosition = GetHomeAzimuthPosition(homeAzimuth, heading);
    }

    private static void UpdateHomeAzimuthPosition(AvaloniaObject source)
    {
        if (source is not AttitudeIndicator indicator)
        {
            return;
        }

        double heading;
        if (indicator.Heading is null)
        {
            heading = double.NaN;
        }
        else
        {
            var isParsed = double.TryParse(indicator.Heading, out heading);
            heading = !isParsed ? double.NaN : heading;
        }

        double.TryParse(indicator.HomeAzimuth, out var homeAzimuth);

        foreach (var item in indicator.HeadingItems)
        {
            item.UpdateValue(heading);
        }

        indicator.HomeAzimuthPosition = GetHomeAzimuthPosition(homeAzimuth, heading);
    }

    private static double GetHomeAzimuthPosition(double? value, double headingValue)
    {
        if (value == null)
        {
            return -100;
        }

        var distance = (headingValue - value.Value) % 360;
        if (distance < -180)
        {
            distance += 360;
        }
        else if (distance > 179)
        {
            distance -= 360;
        }

        return _headingCenterPosition + (distance * _headingPositionStep);
    }
}
