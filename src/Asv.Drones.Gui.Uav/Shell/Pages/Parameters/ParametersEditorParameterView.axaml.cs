using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav;


[ExportView(typeof(ParametersEditorParameterViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class ParametersEditorParameterView : ReactiveUserControl<ParametersEditorParameterViewModel>
{
    public ParametersEditorParameterView()
    {
        InitializeComponent();
    }
}