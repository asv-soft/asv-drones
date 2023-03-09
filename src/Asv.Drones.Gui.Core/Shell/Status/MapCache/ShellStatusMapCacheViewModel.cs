using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IShellStatusItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ShellStatusMapCacheViewModel:ShellStatusItem
    {
        public static readonly Uri Uri = new(ShellStatusItem.Uri,"map-cache");
        
        public ShellStatusMapCacheViewModel() : base(Uri)
        {
            if (Design.IsDesignMode)
            {
                CacheSizeString = "1 024 KB";
            }
        }

        [ImportingConstructor]
        public ShellStatusMapCacheViewModel(IMapService app,ILocalizationService localization):this()
        {
            Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10)).Subscribe(_ =>
            {
                CacheSizeString = localization.ByteSize.GetValueWithUnits(app.CalculateMapCacheSize());
            }).DisposeItWith(Disposable);
            
        }

        

        public override int Order => -1;

        [Reactive]
        public string CacheSizeString { get; set; }
    }
}