using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

[Export(typeof(IShellStatusItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class TextMessageStatusViewModel:ShellStatusItem
{
    public TextMessageStatusViewModel() : base("asv:shell:status:text-message")
    {
    }

    [ImportingConstructor]
    public TextMessageStatusViewModel(ILogService svc):this()
    {
        svc.OnMessage.Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
            .Select(x => new RemoteLogMessageProxy(x)).Subscribe(x=>Message = x)
            .DisposeItWith(Disposable);
    }

    [Reactive]
    public RemoteLogMessageProxy Message { get; set; }
    
    public override int Order => ushort.MinValue;
}