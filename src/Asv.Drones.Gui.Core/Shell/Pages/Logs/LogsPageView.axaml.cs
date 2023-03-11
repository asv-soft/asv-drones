using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(LogsPageViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class LogsPageView : ReactiveUserControl<LogsPageViewModel>
    {
        public LogsPageView()
        {
            InitializeComponent();
        }
    }    
}

