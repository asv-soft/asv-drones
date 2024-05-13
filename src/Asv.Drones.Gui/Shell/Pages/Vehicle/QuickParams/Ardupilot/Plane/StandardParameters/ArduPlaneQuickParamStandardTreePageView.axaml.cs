using System;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(ArduPlaneQuickParamStandardTreePageViewModel))]
public partial class ArduPlaneQuickParamStandardTreePageView : ReactiveUserControl<ArduPlaneQuickParamStandardTreePageViewModel>
{
    public ArduPlaneQuickParamStandardTreePageView()
    {
        InitializeComponent();
    }
}