using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav;

[ExportView(typeof(ParamPageViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class ParamPageView : ReactiveUserControl<ParamPageViewModel>
{
    public ParamPageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}