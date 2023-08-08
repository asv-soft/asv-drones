using System.ComponentModel.Composition;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(CoordinatesCalculatorViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class CoordinatesCalculatorView : ReactiveUserControl<CoordinatesCalculatorViewModel>
{
    public CoordinatesCalculatorView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}