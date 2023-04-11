using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Asv.Cfg;
using Asv.Common;
using Asv.Store;
using LiteDB;
using ReactiveUI;

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
        private readonly string _hostName;
        private ITextStore _store;
        private const string StoreTextCollectionName = "log";

        [ImportingConstructor]
        public LogService(IConfiguration cfgService,IAppService app) : base(cfgService)
        {
            _onMessage = new Subject<LogMessage>().DisposeItWith(Disposable);
            _hostName = $"{Environment.MachineName}.{Environment.UserName}";
            
            app.Store.Subscribe(_ => _store = _.GetText(StoreTextCollectionName)).DisposeItWith(Disposable);
            _onMessage.Subscribe(SaveToStore,DisposeCancel);
        }
        
        private void SaveToStore(LogMessage logMessage)
        {
            _store?.Insert(new TextMessage{Date = logMessage.DateTime, Id = ObjectId.NewObjectId(), IntTag = (int)logMessage.Type, StrTag = GetSourceName(logMessage.Source), Text = logMessage.Message + logMessage.Description });
        }
        
        private string GetSourceName(string rootSource)
        {
            return $"{_hostName}.{rootSource}";
        }

        public IObservable<LogMessage> OnMessage => _onMessage;

        public void SendMessage(LogMessage message)
        {
            _onMessage.OnNext(message);
        }
        
        public ITextStore LogStore => _store;
    }
}