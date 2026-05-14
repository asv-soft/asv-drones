using System;
using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public sealed class TelemetryItemViewModel : RoutableViewModel, ITelemetryItem
{
    public TelemetryItemViewModel(
        string itemId,
        IRoutable content,
        ILoggerFactory loggerFactory,
        params IDisposable[] disposables
    )
        : base(itemId, loggerFactory)
    {
        ItemId = itemId;
        Content = content;

        content.SetRoutableParent(this).DisposeItWith(Disposable);

        foreach (var disposable in disposables)
        {
            disposable.DisposeItWith(Disposable);
        }
    }

    public string ItemId { get; }
    public IRoutable Content { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return Content;
    }
}
