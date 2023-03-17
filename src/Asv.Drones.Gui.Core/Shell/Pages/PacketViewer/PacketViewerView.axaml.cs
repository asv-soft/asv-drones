using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(PacketViewerViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class PacketViewerView : ReactiveUserControl<PacketViewerViewModel>
{
    public PacketViewerView()
    {
        InitializeComponent();
    }
}