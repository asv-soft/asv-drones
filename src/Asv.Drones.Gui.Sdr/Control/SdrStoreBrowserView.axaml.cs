using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Sdr;

[ExportView(typeof(SdrStoreBrowserViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class SdrStoreBrowserView : ReactiveUserControl<SdrStoreBrowserViewModel>
{
    public SdrStoreBrowserView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    
}