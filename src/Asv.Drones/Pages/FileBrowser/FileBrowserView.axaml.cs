using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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

public static class TreeViewBehaviors
{
    public static readonly AttachedProperty<bool> IgnoreEnterProperty =
        AvaloniaProperty.RegisterAttached<TreeView, bool>("IgnoreEnter", typeof(TreeViewBehaviors));

    static TreeViewBehaviors()
    {
        IgnoreEnterProperty.Changed.AddClassHandler<TreeView>(OnIgnoreEnterChanged);
    }

    public static void SetIgnoreEnter(TreeView element, bool value) =>
        element.SetValue(IgnoreEnterProperty, value);

    public static bool GetIgnoreEnter(TreeView element) => element.GetValue(IgnoreEnterProperty);

    private static void OnIgnoreEnterChanged(TreeView tree, AvaloniaPropertyChangedEventArgs e)
    {
        var enabled = e.GetNewValue<bool>();
        if (enabled)
        {
            tree.AddHandler(InputElement.KeyDownEvent, OnTreeKeyDown, RoutingStrategies.Tunnel);
        }
        else
        {
            tree.RemoveHandler(InputElement.KeyDownEvent, OnTreeKeyDown);
        }
    }

    private static void OnTreeKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
        }
    }
}
