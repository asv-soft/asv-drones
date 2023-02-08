using System.ComponentModel.Composition;
using Asv.Avalonia.Map;
using Avalonia;
using Avalonia.Controls;
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

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            
            
            
        }

    }
}
