using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Gbs;

[ExportView(typeof(FlightGbsViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class FlightGbsView : UserControl
{
    public FlightGbsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}