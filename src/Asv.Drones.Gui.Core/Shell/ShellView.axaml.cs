using System.ComponentModel.Composition;
using Asv.Avalonia.Map;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(ShellViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class ShellView : ReactiveUserControl<ShellViewModel>
    {
        public ShellView()
        {
            InitializeComponent();
            
        }

        private void InputElement_OnTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not InfoBadge { DataContext: LogMessageViewModel vm }) return;
            vm.Close();
        }
    }
}
