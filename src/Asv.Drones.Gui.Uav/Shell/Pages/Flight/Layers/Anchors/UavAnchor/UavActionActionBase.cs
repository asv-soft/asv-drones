using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Asv.Avalonia.Map;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using ReactiveUI;

namespace Asv.Drones.Uav
{
    public abstract class UavActionActionBase : MapAnchorActionViewModel,IDisposable
    {
        private readonly IVehicleClient _vehicle;
        private readonly IMap _map;
        private readonly ILogService _log;
        private int _disposeFlag;
        private readonly BehaviorSubject<bool> _canExecuteSubject = new(false);

        protected UavActionActionBase(IVehicleClient vehicle, IMap map,ILogService log)
        {
            _vehicle = vehicle;
            _map = map;
            _log = log;
            var cmd = ReactiveCommand.CreateFromObservable(() => Observable.FromAsync(ExecuteImpl).SubscribeOn(RxApp.TaskpoolScheduler), _canExecuteSubject);
            cmd.ThrownExceptions.Subscribe(OnExecuteError);
            Command = cmd;
            _canExecuteSubject.DisposeWith(Disposable);
        }

        public string LogName => "Map." + Title;

        protected ISubject<bool> CanExecute => _canExecuteSubject;

        protected abstract Task ExecuteImpl(CancellationToken cancel);

        protected virtual void OnExecuteError(Exception ex)
        {
            var sender = $"{_vehicle.Name.Value} {Title}";
            _log.Error(Title, $"{sender} error",ex);
        }

        protected IVehicleClient Vehicle => _vehicle;
        protected IMap Map => _map;
        protected CompositeDisposable Disposable { get; } = new();
        protected virtual void InternalDisposeOnce()
        {
            Disposable.Dispose();
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposeFlag, 1, 0) != 0) return;
            InternalDisposeOnce();
            GC.SuppressFinalize(this);
        }
    }
}