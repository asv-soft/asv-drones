using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using System.Windows.Input;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using DynamicData;
using FluentAvalonia.UI.Controls;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    [Export(typeof(IConnectionPart))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ConnectionsPortsViewModel:ViewModelBase,IConnectionPart
    {
        private readonly IMavlinkDevicesService _deviceSvc;
        private readonly ILogService _logService;
        private readonly ReadOnlyObservableCollection<PortViewModel> _items;

        public ConnectionsPortsViewModel() : base(new Uri(ConnectionsViewModel.BaseUri,"ports"))
        {
            if (Design.IsDesignMode)
            {
                var portList = new List<PortViewModel>
                {
                    new() { Name = "TCP port" },
                    new() { Name = "UDP port" },
                    new() { Name = "Serial port" },
                };
                _items = new ReadOnlyObservableCollection<PortViewModel>(new ObservableCollection<PortViewModel>(portList));


                
            }
        }

        [ImportingConstructor]
        public ConnectionsPortsViewModel(IMavlinkDevicesService deviceSvc, ILogService logService,ILocalizationService localization):this()
        {
            _deviceSvc = deviceSvc ?? throw new ArgumentNullException(nameof(deviceSvc));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            var cache = new SourceCache<PortViewModel, Guid>(_ => _.PortId).DisposeItWith(Disposable);
            cache.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _items)
                .DisposeMany()
                .Subscribe()
                .DisposeItWith(Disposable);
            deviceSvc.Router
                .GetPorts()
                .ForEach(_ => cache.AddOrUpdate(new PortViewModel(deviceSvc, localization, _)));
            deviceSvc.Router
                .OnAddPort.Subscribe(_ => cache.AddOrUpdate(new PortViewModel(deviceSvc, localization, _)))
                .DisposeItWith(Disposable);
            deviceSvc.Router
                .OnRemovePort.Subscribe(_ => cache.Remove(_)).DisposeItWith(Disposable);

            AddSerialPortCommand = ReactiveCommand.CreateFromTask(AddSerialPort).DisposeItWith(Disposable);
            AddTcpPortCommand = ReactiveCommand.CreateFromTask(AddTcpPort).DisposeItWith(Disposable);
            AddUdpPortCommand = ReactiveCommand.CreateFromTask(AddUdpPort).DisposeItWith(Disposable);
        }

        private async Task AddSerialPort(CancellationToken cancel)
        {
            var dialog = new ContentDialog()
            {
                Title = RS.ConnectionsViewModel_AddSerialPortDialog_Title,
                PrimaryButtonText = RS.ConnectionsViewModel_AddDialogPort_Add,
                IsSecondaryButtonEnabled = true,
                CloseButtonText = RS.ConnectionsViewModel_AddDialogPort_Cancel
            };
            var viewModel = new SerialPortViewModel(_deviceSvc, _logService);
            viewModel.ApplyDialog(dialog);
            dialog.Content = viewModel;
            var result = await dialog.ShowAsync();

        }

        private async Task AddTcpPort(CancellationToken cancel)
        {
            var dialog = new ContentDialog()
            {
                Title = RS.ConnectionsViewModel_AddTcpPortDialog_Title,
                PrimaryButtonText = RS.ConnectionsViewModel_AddDialogPort_Add,
                IsSecondaryButtonEnabled = true,
                CloseButtonText = RS.ConnectionsViewModel_AddDialogPort_Cancel
            };
            var viewModel = new TcpPortViewModel(_deviceSvc,_logService);
            viewModel.ApplyDialog(dialog);
            dialog.Content = viewModel;
            var result = await dialog.ShowAsync();

        }

        private async Task AddUdpPort(CancellationToken cancel)
        {
            var dialog = new ContentDialog()
            {
                Title = RS.ConnectionsViewModel_AddUdpPortDialog_Title,
                PrimaryButtonText = RS.ConnectionsViewModel_AddDialogPort_Add,
                IsSecondaryButtonEnabled = true,
                CloseButtonText = RS.ConnectionsViewModel_AddDialogPort_Cancel
            };
            var viewModel = new TcpPortViewModel(_deviceSvc, _logService);
            viewModel.ApplyDialog(dialog);
            dialog.Content = viewModel;
            var result = await dialog.ShowAsync();

        }

        public ReadOnlyObservableCollection<PortViewModel> Items => _items;

        public ICommand AddSerialPortCommand { get; }
        public ICommand AddTcpPortCommand { get; }
        public ICommand AddUdpPortCommand { get; }

        public int Order => 0;
    }
}