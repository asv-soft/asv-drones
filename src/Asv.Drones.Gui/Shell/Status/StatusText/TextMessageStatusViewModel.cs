using System;
using System.Composition;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.StatusText;

[Export(typeof(IShellStatusItem))]
public class TextMessageStatusViewModel : ShellStatusItem
{
    public TextMessageStatusViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
        Message = new RemoteLogMessageProxy(new LogMessage(DateTime.Now, LogMessageType.Error, "Test message", "Test message", "Test message"));
    }

    [ImportingConstructor]
    public TextMessageStatusViewModel(ILogService svc, IApplicationHost host) : base(WellKnownUri.ShellStatusTextMessage)
    {
        svc.OnMessage.Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
            .Select(x => new RemoteLogMessageProxy(x)).Subscribe(x => Message = x)
            .DisposeItWith(Disposable);
        NavigateToSettings = ReactiveCommand.Create(() =>
        {
            host.Shell?.GoTo(WellKnownUri.ShellPageLogViewerUri);
        });
    }

    [Reactive] public RemoteLogMessageProxy Message { get; set; }

    public override int Order => ushort.MinValue;
    public ReactiveCommand<Unit,Unit> NavigateToSettings { get; }
}