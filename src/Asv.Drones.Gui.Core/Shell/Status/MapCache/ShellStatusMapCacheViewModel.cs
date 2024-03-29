﻿using System.ComponentModel.Composition;
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
        public ShellStatusMapCacheViewModel() : base("asv:shell.status.map-cache")
        {
            if (Design.IsDesignMode)
            {
                CacheSizeString = "1 024 KB";
            }
        }

        [ImportingConstructor]
        public ShellStatusMapCacheViewModel(IMapService app,ILocalizationService localization):this()
        {
            Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60)).Subscribe(_ =>
            {
                CacheSizeString = localization.ByteSize.ConvertToStringWithUnits(app.CalculateMapCacheSize());
            }).DisposeItWith(Disposable);
            Description = app.MapCacheDirectory;

        }

        

        public override int Order => -1;

        [Reactive]
        public string CacheSizeString { get; set; }

        public string Description { get; }
    }
}