using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Reactive;
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
        private readonly object _collectionSync = new();
        private readonly Subject<Unit> _onNeedReload;
        private const string StoreTextCollectionName = "log";

        [ImportingConstructor]
        public LogService(IConfiguration cfgService,IAppService app) : base(cfgService)
        {
            _onMessage = new Subject<LogMessage>().DisposeItWith(Disposable);
            _hostName = $"{Environment.MachineName}.{Environment.UserName}";
            _onNeedReload = new Subject<Unit>().DisposeItWith(Disposable);
            app.Store.Subscribe(_ =>
            {
                lock (_collectionSync)
                {
                    _coll = _.Db.GetCollection<LogMessage>(StoreTextCollectionName);
                    _coll.EnsureIndex(x => x.Type);
                    _coll.EnsureIndex(x => x.DateTime);
                    _coll.EnsureIndex(x => x.Source);
                    _coll.EnsureIndex(x => x.Message);
                }
                _onNeedReload.OnNext(Unit.Default);
            }).DisposeItWith(Disposable);
            _onMessage.Subscribe(SaveToStore,DisposeCancel);
        }
        
        private void SaveToStore(LogMessage logMessage)
        {
            lock (_collectionSync)
            {
                logMessage.Source = GetSourceName(logMessage.Source);
                _coll.Insert(logMessage);
            }
        }
        
        private string GetSourceName(string rootSource)
        {
            return $"{_hostName}.{rootSource}";
        }

        public IObservable<LogMessage> OnMessage => _onMessage;
        public void ClearAll()
        {
            lock (_collectionSync)
            {
                _coll.DeleteAll();
            }
            this.Info(nameof(LogService),"User clear all messages");
        }

        public int Count()
        {
            lock (_collectionSync)
            {
                return _coll.Count();
            }
        }

        public IReadOnlyList<LogMessage> Find(LogQuery query)
        {
            lock (_collectionSync)
            {
                return _coll.Find(Convert(query), query.Skip, query.Take).ToImmutableList();
            }
        }

        public int Count(LogQuery query)
        {
            lock (_collectionSync)
            {
                return _coll.Count(Convert(query));
            }
        }

        public IObservable<Unit> OnNeedReload => _onNeedReload;

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