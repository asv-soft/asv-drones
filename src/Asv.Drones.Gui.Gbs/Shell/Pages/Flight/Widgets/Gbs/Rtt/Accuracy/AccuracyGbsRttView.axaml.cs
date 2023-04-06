using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Gbs;

[ExportView(typeof(AccuracyGbsRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class AccuracyGbsRttView : UserControl
{
    public AccuracyGbsRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}