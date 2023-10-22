using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Sdr;

[ExportView(typeof(SdrSettingsViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class SdrSettingsView : ReactiveUserControl<SdrSettingsViewModel>
{
    public SdrSettingsView()
    {
        InitializeComponent();
    }
}