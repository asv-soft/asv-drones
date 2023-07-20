using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Asv.Cfg;
using Asv.Common;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    [ExportShellPage(UriString)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LogMessagesPageViewModel : ShellPage
    {
        public const string UriString = "asv:shell.page.logs";
        public static readonly Uri Uri = new Uri(UriString);

        public List<int> PageLengths { get; }

        private readonly ILogService _logService;
        private readonly IConfiguration _configuration;
        private readonly ObservableAsPropertyHelper<bool> _isRefreshing;
        
        public LogMessagesPageViewModel() : base(Uri)
        {
            Title = RS.FlightShellMenuItem_Name;
            Refresh = ReactiveCommand.CreateFromObservable(() => Observable.Start(RefreshItemsImpl).SubscribeOn(RxApp.TaskpoolScheduler).TakeUntil(CancelRefresh));
            Refresh.IsExecuting.ToProperty(this, _ => _.IsRefreshing, out _isRefreshing);
            Refresh.ThrownExceptions.Subscribe(OnRefreshError);
            CancelRefresh = ReactiveCommand.Create(() => { }, Refresh.IsExecuting);
            ClearAll = ReactiveCommand.CreateFromObservable(() => Observable.Start(ClearAllImpl));
            Next = ReactiveCommand.Create(() =>
            {
                Skip += Take;
            }, CanNext.ObserveOn(RxApp.MainThreadScheduler));
            Prev = ReactiveCommand.Create(() =>
            {
                Skip -= Take;
            }, CanPrev.ObserveOn(RxApp.MainThreadScheduler));    
        }

        [ImportingConstructor]
        public LogMessagesPageViewModel(ILogService logService, IConfiguration configuration) : this()
        {
            _logService = logService;
            _configuration = configuration;
            
            PageLengths = new List<int> { 25, 50, 100, 250, 500 };
            Take = _configuration.Get<int>("TakePageLength");
            if (Take == 0) Take = 25; 
                
            this.WhenAnyValue(_ => _.SearchText)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Throttle(TimeSpan.FromMilliseconds(200),RxApp.MainThreadScheduler)
                .Skip(1)
                .Subscribe(_ => Refresh.Execute())
                .DisposeItWith(Disposable);
            
            this.WhenAnyValue(_ => _.Take, _ => _.Skip)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => Refresh.Execute())
                .DisposeItWith(Disposable);
        }
        
        private void OnRefreshError(Exception ex)
        {
            _logService.Error(RS.LogMessagesPageViewModel_LogName, RS.LogMessagesPageViewModel_RefreshErrorMessage, ex);
        }
        
        private void RefreshItemsImpl()
        {
            if (Skip < 0) Skip = 0;
            var query = new LogQuery { Take = Take, Skip = Skip, Search = SearchText ?? string.Empty };
            
            Messages = _logService.Find(query).Select(_ => new RemoteLogMessageProxy(_)).ToList();

            Filtered = _logService.Count(query);
            
            if (Messages?.Count == 0 && Filtered != 0)
            {
                Skip = 0;
            }
            
            Total = _logService.Count();
            To = Skip + Take;
            CanNext.OnNext(To < Filtered);
            CanPrev.OnNext(Skip > 0);
            _configuration.Set("TakePageLength", Take);
           
        }
        
        private void ClearAllImpl()
        {
            _logService.Warning(RS.LogMessagesPageViewModel_LogName,RS.LogMessagesPageViewModel_ClearAllMessage);
            _logService.ClearAll();
            RefreshItemsImpl();
        }
        
        public bool IsRefreshing => _isRefreshing.Value;
        
        public ReactiveCommand<Unit, Unit> CancelRefresh { get; }
        
        public ReactiveCommand<Unit, Unit> Refresh { get; }

        public ReactiveCommand<Unit,Unit> ClearAll { get; set; }
        
        public ReactiveCommand<Unit, Unit> Next { get; set; }
        
        public ReactiveCommand<Unit, Unit> Prev { get; set; }
        
        public Subject<bool> CanNext { get; } = new();
        
        public Subject<bool> CanPrev { get; } = new();

        [Reactive]
        public List<RemoteLogMessageProxy> Messages { get; set; }

        [Reactive]
        public string SearchText { get; set; }
        
        [Reactive]
        public int Skip { get; set; }
        
        [Reactive]
        public int Take { get; set; }
        
        [Reactive]
        public int Filtered { get; set; }
        
        [Reactive]
        public int Total { get; set; }
        
        [Reactive]
        public int To { get; set; }
        
    }
    
    public class RemoteLogMessageProxy
    {
        public RemoteLogMessageProxy(LogMessage textMessage)
        {
            switch (textMessage.Type)
            {
                case LogMessageType.Info:
                    IsInfo = true;
                    Icon = MaterialIconKind.InformationCircle;
                    break;
                
                case LogMessageType.Warning:
                    IsWarning = true;
                    Icon = MaterialIconKind.Warning;
                    break;
                
                case LogMessageType.Error:
                    IsError = true;
                    Icon = MaterialIconKind.Warning;
                    break;
                
                case LogMessageType.Trace:
                    IsTrace = true;
                    Icon = MaterialIconKind.Exclamation;
                    break;
            }

            DateTime = textMessage.DateTime;
            Sender = textMessage.Source;
            Message = textMessage.Message;
        }
        
        public bool IsError { get; }
        public bool IsWarning { get; }
        public bool IsTrace { get; }
        public bool IsInfo { get; }

        public DateTime DateTime { get; }
        public MaterialIconKind Icon { get; }
        public string Sender { get; }
        public string Message { get; }
    }
}

