using Avalonia.Layout;
using Avalonia.ReactiveUI;
using DynamicData.Binding;

namespace Asv.Drones.Gui.Api;

[ExportView(typeof(AnchorsEditorViewModel))]
public partial class AnchorsEditorView : ReactiveUserControl<AnchorsEditorViewModel>
{
    private IDisposable? _subscribe;
    private double? _lastWidth = null;

    public AnchorsEditorView()
    {
        InitializeComponent();
        this.WhenValueChanged(x => x.ViewModel).Subscribe(OnViewModel);
    }


    private void OnViewModel(AnchorsEditorViewModel? vm)
    {
        if (vm == null) return;
        _subscribe?.Dispose();
    }

    private void Layoutable_OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        ViewModel.IsCompactMode = PART_Grid.Bounds.Width switch
        {
            < 500 => true,
            > 500 => false,
            _ => ViewModel.IsCompactMode
        };
    }
}