using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav;

[ExportView(typeof(QuickParamsSetupPageViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class QuickParamsSetupPageView : ReactiveUserControl<QuickParamsSetupPageViewModel>
{
    public QuickParamsSetupPageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}