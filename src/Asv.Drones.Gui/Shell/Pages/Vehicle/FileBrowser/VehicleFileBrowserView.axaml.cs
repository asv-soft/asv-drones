using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Asv.Drones.Gui.Api;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Avalonia.Xaml.Interactivity;

namespace Asv.Drones.Gui;

[ExportView(typeof(VehicleFileBrowserViewModel))]
public partial class VehicleFileBrowserView : ReactiveUserControl<VehicleFileBrowserViewModel>
{
    public VehicleFileBrowserView()
    {
        InitializeComponent();
    }
    
    private void TreeView_GotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is not TreeView treeView) return;
        if (treeView.SelectedItem == null && treeView.Items.Count > 0)
        {
            treeView.SelectedItem = treeView.Items[0];
        }
    }
}

public class EnterKeyBehavior : Behavior<TextBox>
{
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<EnterKeyBehavior, ICommand?>(nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject != null) AssociatedObject.KeyDown += OnKeyDown;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject != null) AssociatedObject.KeyDown -= OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || Command?.CanExecute(null) != true) return;
        Command.Execute(null);
        e.Handled = true;
    }
}