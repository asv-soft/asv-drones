using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav;

[ExportView(typeof(ParametersEditorPageViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class ParametersEditorPageView : ReactiveUserControl<ParametersEditorPageViewModel>
{
    public ParametersEditorPageView()
    {
        InitializeComponent();
    }
}