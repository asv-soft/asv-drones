using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(ShellStatusMapCacheViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ShellStatusMapCacheView : ReactiveUserControl<ShellStatusMapCacheViewModel>
    {
        public ShellStatusMapCacheView()
        {
            InitializeComponent();
        }
    }
}
