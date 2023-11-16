using System.ComponentModel.Composition;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(ParamItemViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class ParamItemView : ReactiveUserControl<ParamItemViewModel>
{
    public ParamItemView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}