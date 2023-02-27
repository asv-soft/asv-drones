using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IShellStatusItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ShellStatusMapCacheViewModel:ViewModelBase, IShellStatusItem
    {
        public ShellStatusMapCacheViewModel() : base(new(WellKnownUri.ShellStatusUri,"mapcahce"))
        {
            if (Design.IsDesignMode)
            {
                CacheSizeString = "1 024 KB";
            }
        }

        [ImportingConstructor]
        public ShellStatusMapCacheViewModel(IAppService app,ILocalizationService localization):this()
        {
            var mapDir = new DirectoryInfo(app.Paths.MapCacheFolder);
            Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10)).Subscribe(_ =>
            {
                CacheSizeString = localization.BytesToString(DirSize(mapDir));
            }).DisposeItWith(Disposable);
            
        }

        private static long DirSize(DirectoryInfo d)
        {
            // Add file sizes.
            var fis = d.GetFiles();
            var size = fis.Sum(fi => fi.Length);
            // Add subdirectory sizes.
            var dis = d.GetDirectories();
            size += dis.Sum(DirSize);
            return size;
        }

        public int Order => 0;

        [Reactive]
        public string CacheSizeString { get; set; }
    }
}