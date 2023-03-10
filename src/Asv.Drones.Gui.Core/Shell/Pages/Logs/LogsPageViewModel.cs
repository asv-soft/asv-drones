using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Asv.Cfg;
using Asv.Common;
using Asv.Store;
using DynamicData;
using LiteDB;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    [ExportShellPage(UriString)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LogsPageViewModel : ViewModelBase, IShellPage
    {
        public const string UriString = ShellPage.UriString + ".logs";
        public static readonly Uri Uri = new Uri(UriString);

        public List<int> PageLengths { get; }

        private readonly ILogService _logService;
        private readonly IConfiguration _configuration;
        private readonly ObservableAsPropertyHelper<bool> _isRefreshing;
        
        public LogsPageViewModel() : base(Uri)
        {
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
        public LogsPageViewModel(ILogService logService, IConfiguration configuration) : this()
        {
            _logService = logService;
            _configuration = configuration;
            
            this.WhenAnyValue(_ => _.SearchText).Throttle(TimeSpan.FromMilliseconds(200)).Skip(1).Subscribe(_ => Refresh.Execute()).DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.Take, _ => _.Skip).Subscribe(_ => Refresh.Execute()).DisposeItWith(Disposable);

            PageLengths = new List<int> { 25, 50, 100, 250, 500 };

            Take = _configuration.Get<int>("TakePageLength");
        }
        
        private void OnRefreshError(Exception ex)
        {
            _logService.Error("LogView", "Error to refresh items", ex);
        }
        
        private void RefreshItemsImpl()
        {
            if (Skip < 0) Skip = 0;
            var query = new TextMessageQuery { Take = Take, Skip = Skip, Search = SearchText ?? "" };
            Messages = _logService.LogStore.Find(query).Distinct().ToList();
            Filtered = _logService.LogStore.Count(query);
            if (Messages.Count == 0 & Filtered != 0)
            {
                Skip = 0;
                RefreshItemsImpl();
            }
            Total = _logService.LogStore.Count();
            To = Skip + Take;
            CanNext.OnNext(To < Filtered);
            CanPrev.OnNext(Skip > 0);
            _configuration.Set("TakePageLength", Take);
        }
        
        private void ClearAllImpl()
        {
            _logService.Warning("LogMessages","Clear all log messages");
            _logService.LogStore.ClearAll();
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
        public List<TextMessage> Messages { get; set; }
        
        [Reactive]
        public string SearchText { get; set; }
        
        [Reactive]
        public int Skip { get; set; } = 0;
        
        [Reactive]
        public int Take { get; set; } = 50;
        
        [Reactive]
        public int Filtered { get; set; }
        
        [Reactive]
        public int Total { get; set; }
        
        [Reactive]
        public int To { get; set; }
        
        public void SetArgs(Uri link)
        {
            
        }
    }    
}

