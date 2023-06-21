using System;
using System.Diagnostics;
using NLog;

namespace Asv.Drones.Gui;

public class NlogExceptionHandler : IObserver<Exception>
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    public void OnNext(Exception e)
    {
        if (Debugger.IsAttached) Debugger.Break();
        _logger.Fatal(e, "Unhandled RxApp exception:{0}", e.Message);
        //RxApp.MainThreadScheduler.Schedule(() => throw e);
    }

    public void OnError(Exception e)
    {
        if (Debugger.IsAttached) Debugger.Break();
        _logger.Fatal(e, "Unhandled RxApp exception:{0}", e.Message);
        //RxApp.MainThreadScheduler.Schedule(() => throw e);
    }

    public void OnCompleted()
    {
        if (Debugger.IsAttached) Debugger.Break();
        //RxApp.MainThreadScheduler.Schedule(() => throw new NotImplementedException());
    }
}