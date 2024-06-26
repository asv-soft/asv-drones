﻿using Asv.Drones.Gui.Api;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using System;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Asv.Drones.Gui;

[ExportView(typeof(QuickParametersSliderViewModel))]
public partial class QuickParametersSliderView : ReactiveUserControl<QuickParametersSliderViewModel>
{
    public QuickParametersSliderView()
    {
        InitializeComponent();
    }

    private void NumUpDown_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (sender is not NumericUpDown numericUpDown) return;
        if (!string.IsNullOrEmpty(numericUpDown.Text)) return;
        
        numericUpDown.Value = numericUpDown.Minimum;
        numericUpDown.Text = numericUpDown.Value.ToString();
    }
}
