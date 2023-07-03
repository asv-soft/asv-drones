using FluentAvalonia.UI.Controls;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

public class SeparatorViewModel : ViewModelBaseWithValidation
{
    public const string UriString = PacketViewerViewModel.UriString + ".dialogs.separator";
    public static readonly Uri Uri = new(UriString);

    public SeparatorViewModel() : base(Uri)
    {
        IsSemicolon = true;
    }
    
    public void ApplyDialog(ContentDialog dialog)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));
    }

    [Reactive]
    public bool IsSemicolon { get; set; }
    
    [Reactive]
    public bool IsComa { get; set; }

    [Reactive]
    public bool IsTab { get; set; }

}