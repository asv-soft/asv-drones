using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    [ExportView(typeof(ShellStatusPortsViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ShellStatusPortsView : ReactiveUserControl<ShellStatusPortsViewModel>
    {
        public ShellStatusPortsView()
        {
            InitializeComponent();
        }
    }
}
