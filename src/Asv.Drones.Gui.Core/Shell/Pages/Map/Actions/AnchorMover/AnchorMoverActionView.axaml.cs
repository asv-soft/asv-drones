using System.ComponentModel.Composition;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(AnchorMoverActionViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class AnchorMoverActionView : ReactiveUserControl<AnchorMoverActionViewModel>
{
    [ImportingConstructor]
    public AnchorMoverActionView()
    {
        InitializeComponent();
        HotKeyManager.SetHotKey(editAnchorsToggleButton, new KeyGesture(Key.LeftCtrl, KeyModifiers.Control));
    }
}