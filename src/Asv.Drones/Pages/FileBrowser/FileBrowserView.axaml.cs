using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Asv.Drones;

[ExportViewFor(typeof(FileBrowserViewModel))]
public partial class FileBrowserView : UserControl
{
    public FileBrowserView()
    {
        InitializeComponent();
    }

    private void TreeView_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not TreeView tree)
        {
            return;
        }

        var hit = tree.InputHitTest(e.GetPosition(tree));

        if (IsInsideTreeViewItem(hit))
        {
            return;
        }

        tree.SelectedItem = null;
    }

    private static bool IsInsideTreeViewItem(IInputElement? hit)
    {
        var v = hit as Visual;
        while (v is not null && v is not TreeViewItem)
        {
            v = v.GetVisualParent();
        }

        return v is TreeViewItem;
    }
}
