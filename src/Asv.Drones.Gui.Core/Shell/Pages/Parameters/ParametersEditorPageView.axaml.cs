using System.ComponentModel.Composition;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(ParametersEditorPageViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class ParametersEditorPageView : ReactiveUserControl<ParametersEditorPageViewModel>
{
    public ParametersEditorPageView()
    {
        InitializeComponent();
    }
}