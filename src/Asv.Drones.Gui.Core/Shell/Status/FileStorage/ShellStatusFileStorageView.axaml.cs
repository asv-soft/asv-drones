using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(ShellStatusFileStorageViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ShellStatusFileStorageView : ReactiveUserControl<ShellStatusFileStorageViewModel>
    {
        public ShellStatusFileStorageView()
        {
            InitializeComponent();
        }
    }
}
