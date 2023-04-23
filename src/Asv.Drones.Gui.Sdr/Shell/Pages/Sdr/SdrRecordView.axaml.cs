using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Sdr;

[ExportView(typeof(SdrRecordViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class SdrRecordView : ReactiveUserControl<SdrRecordViewModel>
{
    public SdrRecordView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}