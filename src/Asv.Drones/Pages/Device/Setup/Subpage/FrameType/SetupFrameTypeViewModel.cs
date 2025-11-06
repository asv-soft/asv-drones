using System;
using System.Composition;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

[ExportSetup(PageId)]
public sealed class SetupFrameTypeViewModel : SetupSubpage
{
    public const string PageId = "frame_type";

    [ImportingConstructor]
    public SetupFrameTypeViewModel(ILoggerFactory loggerFactory)
        : base(PageId, loggerFactory)
    {
        FullIdDebug = new BindableReactiveProperty<string>(
            NavigationId.NormalizeTypeId(this.GetPathToRoot().ToString())
        ).DisposeItWith(Disposable);
        SomeText = new BindableReactiveProperty<string?>(null).DisposeItWith(Disposable);

        Observable
            .Timer(TimeSpan.FromMilliseconds(500))
            .Subscribe(_ =>
                FullIdDebug.Value = NavigationId.NormalizeTypeId(this.GetPathToRoot().ToString())
            )
            .DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<string> FullIdDebug { get; }
    public BindableReactiveProperty<string?> SomeText { get; }

    public override IExportInfo Source => SystemModule.Instance;
}
