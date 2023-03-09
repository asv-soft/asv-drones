using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Core;
using Avalonia.Controls;
using LiteDB;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

[Export(typeof(IShellStatusItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class ShellStatusFileStorageViewModel : ShellStatusItem
{
    public static readonly Uri Uri = new(ShellStatusItem.Uri, "file-storage");
    public ShellStatusFileStorageViewModel() : base(Uri)
    {
        if (Design.IsDesignMode)
        {
            Title = "MissionFile.asv";
            StorageSize = "2 048 KB";
        }
    }

    [ImportingConstructor]
    public ShellStatusFileStorageViewModel(IAppService app, ILocalizationService localizationService) :this()
    {
       app.Store.Where(_ => _ != null)
            .Select(_ => Path.GetFileNameWithoutExtension(Path.GetFileName(_.SourceName)))
            .Subscribe(_ => Title = _)
            .DisposeItWith(Disposable);

        var storageSize = app.Store.Value.GetFileSizeInBytes();

        StorageSize = localizationService.ByteSize.ConvertToStringWithUnits(storageSize);
    }

    public override int Order => -3;

    [Reactive]
    public string Title { get; set; }
    [Reactive]
    public string StorageSize { get; set; }
}