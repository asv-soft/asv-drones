using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

[Export(typeof(IShellStatusItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class ShellStatusFileStorageViewModel : ShellStatusItem
{
    public ShellStatusFileStorageViewModel() : base("asv:shell.status.file-storage")
    {
        if (Design.IsDesignMode)
        {
            StorageSize = "2 048 KB";
        }
    }

    [ImportingConstructor]
    public ShellStatusFileStorageViewModel(IAppService app, ILocalizationService localizationService) :this()
    {
        Description = app.Store.SourceName;
        Observable.Timer(TimeSpan.FromSeconds(1),TimeSpan.FromSeconds(60))
            .Select(_ => app.Store.GetFileSizeInBytes())
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_=>StorageSize = localizationService.ByteSize.ConvertToStringWithUnits(_))
            .DisposeItWith(Disposable);
    }

    public override int Order => -3;
    [Reactive]
    public string StorageSize { get; set; }

    public string Description { get; }
}