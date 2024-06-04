using System;
using Asv.Drones.Gui.Api;
using FluentAvalonia.UI.Controls;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class SeparatorViewModel : DisposableReactiveObject
{
    public SeparatorViewModel()
    {
        IsSemicolon = true;
    }

    public void ApplyDialog(ContentDialog dialog)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));
    }

    [Reactive] public bool IsSemicolon { get; set; }

    [Reactive] public bool IsComa { get; set; }

    [Reactive] public bool IsTab { get; set; }
}