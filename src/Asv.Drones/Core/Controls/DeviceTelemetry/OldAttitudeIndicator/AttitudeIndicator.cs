using System;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using R3;

namespace Asv.Drones.OldAttitudeIndicator;

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

        var smallerSide = Math.Min((double)InternalWidth, InternalHeight);
        Scale = smallerSide / 100;

        RollItems = new AvaloniaList<OldAttitudeIndicator.RollItem>(
            new OldAttitudeIndicator.RollItem(0),
            new OldAttitudeIndicator.RollItem(10),
            new OldAttitudeIndicator.RollItem(20),
            new OldAttitudeIndicator.RollItem(30),
            new OldAttitudeIndicator.RollItem(45),
            new OldAttitudeIndicator.RollItem(60),
            new OldAttitudeIndicator.RollItem(300),
            new OldAttitudeIndicator.RollItem(315),
            new OldAttitudeIndicator.RollItem(330),
            new OldAttitudeIndicator.RollItem(340),
            new OldAttitudeIndicator.RollItem(350)
        );

        PitchItems = new AvaloniaList<OldAttitudeIndicator.PitchItem>(
            new OldAttitudeIndicator.PitchItem(135, Scale, false),
            new OldAttitudeIndicator.PitchItem(130, Scale),
            new OldAttitudeIndicator.PitchItem(125, Scale, false),
            new OldAttitudeIndicator.PitchItem(120, Scale),
            new OldAttitudeIndicator.PitchItem(115, Scale, false),
            new OldAttitudeIndicator.PitchItem(110, Scale),
            new OldAttitudeIndicator.PitchItem(105, Scale, false),
            new OldAttitudeIndicator.PitchItem(100, Scale),
            new OldAttitudeIndicator.PitchItem(95, Scale, false),
            new OldAttitudeIndicator.PitchItem(90, Scale),
            new OldAttitudeIndicator.PitchItem(85, Scale, false),
            new OldAttitudeIndicator.PitchItem(80, Scale),
            new OldAttitudeIndicator.PitchItem(75, Scale, false),
            new OldAttitudeIndicator.PitchItem(70, Scale),
            new OldAttitudeIndicator.PitchItem(65, Scale, false),
            new OldAttitudeIndicator.PitchItem(60, Scale),
            new OldAttitudeIndicator.PitchItem(55, Scale, false),
            new OldAttitudeIndicator.PitchItem(50, Scale),
            new OldAttitudeIndicator.PitchItem(45, Scale, false),
            new OldAttitudeIndicator.PitchItem(40, Scale),
            new OldAttitudeIndicator.PitchItem(35, Scale, false),
            new OldAttitudeIndicator.PitchItem(30, Scale),
            new OldAttitudeIndicator.PitchItem(25, Scale, false),
            new OldAttitudeIndicator.PitchItem(20, Scale),
            new OldAttitudeIndicator.PitchItem(15, Scale, false),
            new OldAttitudeIndicator.PitchItem(10, Scale),
            new OldAttitudeIndicator.PitchItem(5, Scale, false),
            new OldAttitudeIndicator.PitchItem(0, Scale),
            new OldAttitudeIndicator.PitchItem(-5, Scale, false),
            new OldAttitudeIndicator.PitchItem(-10, Scale),
            new OldAttitudeIndicator.PitchItem(-15, Scale, false),
            new OldAttitudeIndicator.PitchItem(-20, Scale),
            new OldAttitudeIndicator.PitchItem(-25, Scale, false),
            new OldAttitudeIndicator.PitchItem(-30, Scale),
            new OldAttitudeIndicator.PitchItem(-35, Scale, false),
            new OldAttitudeIndicator.PitchItem(-40, Scale),
            new OldAttitudeIndicator.PitchItem(-45, Scale, false),
            new OldAttitudeIndicator.PitchItem(-50, Scale),
            new OldAttitudeIndicator.PitchItem(-55, Scale, false),
            new OldAttitudeIndicator.PitchItem(-60, Scale),
            new OldAttitudeIndicator.PitchItem(-65, Scale, false),
            new OldAttitudeIndicator.PitchItem(-70, Scale),
            new OldAttitudeIndicator.PitchItem(-75, Scale, false),
            new OldAttitudeIndicator.PitchItem(-80, Scale),
            new OldAttitudeIndicator.PitchItem(-85, Scale, false),
            new OldAttitudeIndicator.PitchItem(-90, Scale),
            new OldAttitudeIndicator.PitchItem(-95, Scale, false),
            new OldAttitudeIndicator.PitchItem(-100, Scale),
            new OldAttitudeIndicator.PitchItem(-105, Scale, false),
            new OldAttitudeIndicator.PitchItem(-110, Scale),
            new OldAttitudeIndicator.PitchItem(-115, Scale, false),
            new OldAttitudeIndicator.PitchItem(-120, Scale),
            new OldAttitudeIndicator.PitchItem(-125, Scale, false),
            new OldAttitudeIndicator.PitchItem(-130, Scale),
            new OldAttitudeIndicator.PitchItem(-135, Scale, false)
        );

        var velocityControlLength = smallerSide * VelocityControlLengthPrc;
        var velocityItemLength = velocityControlLength / (VelocityItemCount - 1);
        VelocityItems = new AvaloniaList<OldAttitudeIndicator.ScaleItem>(
            Enumerable
                .Range(0, VelocityItemCount)
                .Select(_ => new OldAttitudeIndicator.ScaleItem(
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
        AltitudeItems = new AvaloniaList<OldAttitudeIndicator.ScaleItem>(
            Enumerable
                .Range(0, AltitudeItemCount)
                .Select(_ => new OldAttitudeIndicator.ScaleItem(
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
        HeadingItems = new AvaloniaList<OldAttitudeIndicator.ScaleItem>(
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
        if (change.Property == AttitudeIndicator.VibrationXProperty)
        {
            UpdateColorX(change.Sender);
        }
        else if (change.Property == AttitudeIndicator.VibrationYProperty)
        {
            UpdateColorY(change.Sender);
        }
        else if (change.Property == AttitudeIndicator.VibrationZProperty)
        {
            UpdateColorZ(change.Sender);
        }
        else if (change.Property == AttitudeIndicator.VelocityProperty)
        {
            UpdateVelocityItems(change.Sender);
        }
        else if (change.Property == AttitudeIndicator.RollAngleProperty)
        {
            UpdateRollAngle(change.Sender);
        }
        else if (change.Property == AttitudeIndicator.PitchAngleProperty)
        {
            UpdateAngle(change.Sender);
        }
        else if (change.Property == AttitudeIndicator.AltitudeProperty)
        {
            UpdateAltitudeItems(change.Sender);
        }
        else if (change.Property == AttitudeIndicator.HeadingProperty)
        {
            UpdateHeadingItems(change.Sender);
        }
        else if (change.Property == AttitudeIndicator.HomeAzimuthProperty)
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

        foreach (var item in indicator.VelocityItems)
        {
            item.UpdateValue(indicator.Velocity);
        }
    }

    private static void UpdateAltitudeItems(AvaloniaObject source)
    {
        if (source is not AttitudeIndicator indicator)
        {
            return;
        }

        foreach (var item in indicator.AltitudeItems)
        {
            item.UpdateValue(indicator.Altitude);
        }
    }

    private static void UpdateHeadingItems(AvaloniaObject source)
    {
        if (source is not AttitudeIndicator indicator)
        {
            return;
        }

        foreach (var item in indicator.HeadingItems)
        {
            item.UpdateValue(indicator.Heading);
        }

        indicator.HomeAzimuthPosition = GetHomeAzimuthPosition(
            indicator.HomeAzimuth,
            indicator.Heading
        );
    }

    private static void UpdateHomeAzimuthPosition(AvaloniaObject source)
    {
        if (source is not AttitudeIndicator indicator)
        {
            return;
        }

        foreach (var item in indicator.HeadingItems)
        {
            item.UpdateValue(indicator.Heading);
        }

        indicator.HomeAzimuthPosition = GetHomeAzimuthPosition(
            indicator.HomeAzimuth,
            indicator.Heading
        );
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
