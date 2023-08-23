using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

public class MoveDialogViewModel : ViewModelBase
{
    public const string UriString = "asv:shell.page.sdr-store" + ".dialogs.move";
    public static readonly Uri Uri = new(UriString);
    
    private readonly ReadOnlyObservableCollection<SdrStoreEntityViewModel> _tree;

    public MoveDialogViewModel(SdrStoreBrowserViewModel context) : base(Uri)
    {
        _tree = context.Items;
    }

    public ReadOnlyObservableCollection<SdrStoreEntityViewModel> Items => _tree;
    
    [Reactive]
    public SdrStoreEntityViewModel SelectedItem { get; set; }
}