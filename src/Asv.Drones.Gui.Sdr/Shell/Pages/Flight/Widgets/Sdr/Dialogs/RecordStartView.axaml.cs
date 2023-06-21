using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Sdr;

[ExportView(typeof(RecordStartViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class RecordStartView : ReactiveUserControl<RecordStartViewModel>
{
    public RecordStartView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}