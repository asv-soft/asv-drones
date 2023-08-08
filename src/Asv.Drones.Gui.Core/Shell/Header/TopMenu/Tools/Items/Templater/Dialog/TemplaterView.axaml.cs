using System.ComponentModel.Composition;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(TemplaterViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class TemplaterView : ReactiveUserControl<TemplaterViewModel>
{
    public TemplaterView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}