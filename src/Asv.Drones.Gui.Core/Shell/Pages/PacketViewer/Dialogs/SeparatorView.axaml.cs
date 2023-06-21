using System.ComponentModel.Composition;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(SeparatorViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class SeparatorView : ReactiveUserControl<SeparatorViewModel>
{
    public SeparatorView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}