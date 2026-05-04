using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Primitives;

namespace Asv.Drones;

public partial class UavAngleIndicator : TemplatedControl
{
    public UavAngleIndicator()
    {
        Scale = Math.Min(InternalWidth, InternalHeight) / 100;

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
    }

    public double Scale { get; }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == RollAngleProperty)
        {
            UpdateRollAngle(change.Sender);
        }
        else if (change.Property == PitchAngleProperty)
        {
            UpdateAngle(change.Sender);
        }
    }

    private static void UpdateAngle(AvaloniaObject source)
    {
        if (source is not UavAngleIndicator indicator)
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
        if (source is not UavAngleIndicator indicator)
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
}
