using System;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(ArduCopterQuickParamStandardTreePageViewModel))]
public partial class ArduCopterQuickParamStandardTreePageView : ReactiveUserControl<ArduCopterQuickParamStandardTreePageViewModel>
{
    public ArduCopterQuickParamStandardTreePageView()
    {
        InitializeComponent();
    }
}