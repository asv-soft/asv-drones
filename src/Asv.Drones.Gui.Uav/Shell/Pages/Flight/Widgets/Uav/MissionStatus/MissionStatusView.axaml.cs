using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav.MissionStatus;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav.Uav;

[ExportView(typeof(MissionStatusViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class MissionStatusView : UserControl
{
    public MissionStatusView()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
}