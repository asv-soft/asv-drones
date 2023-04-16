using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav;

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