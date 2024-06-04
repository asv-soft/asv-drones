using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Api;

[ExportView(typeof(ParamPageViewModel))]
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