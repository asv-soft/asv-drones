using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using Asv.Cfg;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Input;
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