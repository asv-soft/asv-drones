using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(LogMessagesPageViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class LogMessagesPageView : ReactiveUserControl<LogMessagesPageViewModel>
    {
        public LogMessagesPageView()
        {
            InitializeComponent();
        }
    }    
}

