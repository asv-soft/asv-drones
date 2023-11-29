using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Reactive.Subjects;
using Asv.Cfg;
using Asv.Common;
using LiteDB;

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
        private ILiteCollection<LogMessage> _coll;
        private const string StoreTextCollectionName = "log";

        [ImportingConstructor]
        public LogService(IConfiguration cfgService,IAppService app) : base(cfgService)
        {
            _onMessage = new Subject<LogMessage>().DisposeItWith(Disposable);
            _hostName = $"{Environment.MachineName}.{Environment.UserName}";
            _onMessage.Subscribe(SaveToStore,DisposeCancel);
            _coll = app.Store.Db.GetCollection<LogMessage>(StoreTextCollectionName);
            _coll.EnsureIndex(x => x.Type);
            _coll.EnsureIndex(x => x.DateTime);
            _coll.EnsureIndex(x => x.Source);
            _coll.EnsureIndex(x => x.Message);
        }
        
        private void SaveToStore(LogMessage logMessage)
        {
            logMessage.Source = GetSourceName(logMessage.Source);
            _coll.Insert(logMessage);
        }
        
        private string GetSourceName(string rootSource)
        {
            return rootSource;
            return $"{_hostName}.{rootSource}";
        }

        public IObservable<LogMessage> OnMessage => _onMessage;
        public void ClearAll()
        {
            _coll.DeleteAll();
            this.Info(nameof(LogService),"User clear all messages");
        }

        public int Count()
        {
            return _coll.Count();
        }

        public IEnumerable<LogMessage> Find(LogQuery query)
        {
            return _coll.Find(Convert(query), query.Skip, query.Take).ToImmutableList();
        }

        public int Count(LogQuery query)
        {
            return _coll.Count(Convert(query));
        }
        public void SaveMessage(LogMessage message)
        {
            _onMessage.OnNext(message);
        }
        private static Query Convert(LogQuery query)
        {
            var q = Query.All(nameof(LogMessage.DateTime));

            if (!query.Search.IsNullOrWhiteSpace())
            {
                q.Where.Add(Query.Contains(nameof(LogMessage.Message), query.Search));
            }

            return q;
        }
        
    }
}