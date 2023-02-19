using System.ComponentModel.Composition;
using System.Reactive.Subjects;
using Asv.Cfg;
using Asv.Common;

namespace Asv.Drones.Gui.Core
{
    public class LogServiceConfig
    {

    }

    [Export(typeof(ILogService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LogService : ServiceWithConfigBase<LogServiceConfig>, ILogService
    {
        private readonly Subject<LogMessage> _onMessage;

        [ImportingConstructor]
        public LogService(IConfiguration cfgService) : base(cfgService)
        {
            _onMessage = new Subject<LogMessage>().DisposeItWith(Disposable);
        }

        public IObservable<LogMessage> OnMessage => _onMessage;

        public void SendMessage(LogMessage message)
        {
            _onMessage.OnNext(message);
        }
    }
}