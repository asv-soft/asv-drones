using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui
{
    [ExportView(typeof(ShellStatusMapCacheViewModel))]
    public partial class ShellStatusMapCacheView : ReactiveUserControl<ShellStatusMapCacheViewModel>
    {
        public ShellStatusMapCacheView()
        {
            InitializeComponent();
        }
    }
}