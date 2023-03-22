using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Uav;

[ExportView(typeof(CurrentUavRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class CurrentUavRttView : UserControl
{
    public CurrentUavRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}