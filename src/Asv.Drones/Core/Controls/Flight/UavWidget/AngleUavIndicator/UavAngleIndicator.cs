// TODO: asv-soft-u08

using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Primitives;

namespace Asv.Drones.AngleIndicator;

public partial class UavAngleIndicator : TemplatedControl
{
    public UavAngleIndicator()
    {
        Scale = Math.Min(InternalWidth, InternalHeight) / 100;

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