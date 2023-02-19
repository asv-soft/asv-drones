using System.Drawing;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.IO;
using Asv.Mavlink;
using Avalonia.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav
{
    public class PortViewModel:ViewModelBase
    {
        private readonly IMavlinkDevicesService _svc;
        private readonly Guid _id;
        private long _lastRx = long.MaxValue;
        private long _lastTx = long.MaxValue;
        private DateTime _lastUpdate = DateTime.MaxValue;
        private long _lastRxPkt = long.MaxValue;
        private long _lastTxPkt = long.MaxValue;

        public PortViewModel() : base(new Uri(ConnectionsViewModel.BaseUri, $"port.{Guid.NewGuid()}"))
        {
            if (Design.IsDesignMode)
            {
                PortId = Guid.NewGuid();
                Icon = ConvertIcon(PortType.Serial);
                RxText = "123.456";
                TxText = "123.456";
                ConnectionString = "tcp://127.0.0.1:7341?srv=true";
            }
            
        }

        public PortViewModel(Guid id):base(new Uri(ConnectionsViewModel.BaseUri, $"port.{id}"))
        {
            PortId = id;
            
        }

        public PortViewModel(IMavlinkDevicesService svc,Guid id):this(id)
        {
            _svc = svc;
            _id = id;
            Observable.Timer(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(1)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(Update).DisposeItWith(Disposable);
            EnableDisableCommand = ReactiveCommand.Create(() => _svc.Router.SetEnabled(_id, IsPortEnabled)).DisposeItWith(Disposable);
            DeletePortCommand = ReactiveCommand.Create(() => _svc.Router.RemovePort(_id)).DisposeItWith(Disposable);
            Disposable.AddAction(() =>
            {

            });
        }

        public Guid PortId { get; }

        private void Update(long l)
        {
            var info = _svc.Router.GetInfo(_id);
            if (info == null) return;
            Name = info.Name;
            Icon = ConvertIcon(info.Type);
            ConnectionString = info.ConnectionString;
            Description = info.Description;
            switch (info.State)
            {
                case PortState.Disabled:
                    IsDisabled = true;
                    IsConnected = false;
                    IsConnecting = false;
                    IsError = false;
                    break;
                case PortState.Connecting:
                    IsDisabled = false;
                    IsConnected = false;
                    IsConnecting = true;
                    IsError = false;
                    break;
                case PortState.Error:
                    IsDisabled = false;
                    IsConnected = false;
                    IsConnecting = false;
                    IsError = true;
                    break;
                case PortState.Connected:
                    IsDisabled = false;
                    IsConnected = true;
                    IsConnecting = false;
                    IsError = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Error = info.LastException?.Message;

            var now = DateTime.Now;

            var deltaSeconds = (now - _lastUpdate).TotalSeconds;
            if (deltaSeconds <= 0) deltaSeconds = 1;

            var deltaRx = info.RxBytes - _lastRx;
            if (deltaRx <= 0) deltaRx = 0;

            var deltaRxPkt = info.RxPackets - _lastRxPkt;
            if (deltaRxPkt <= 0) deltaRxPkt = 0;

            var deltaTx = info.TxBytes - _lastTx;
            if (deltaTx <= 0) deltaTx = 0;

            var deltaTxPkt = info.TxPackets - _lastTxPkt;
            if (deltaTxPkt <= 0) deltaTxPkt = 0;


            RxText = $"{deltaRx / deltaSeconds / 1024.0:F3} / {deltaRxPkt,-3}";
            TxText = $"{deltaTx / deltaSeconds / 1024.0:F3} / {deltaTxPkt,-3}";
            IsPortEnabled = info.IsEnabled;
            SkippedText = $"{info.SkipPackets}/{info.DeserializationErrors}";


            _lastRx = info.RxBytes;
            _lastTx = info.TxBytes;
            _lastRxPkt = info.RxPackets;
            _lastTxPkt = info.TxPackets;
            _lastUpdate = now;
        }

        

        private MaterialIconKind ConvertIcon(PortType infoType)
        {
            return infoType switch
            {
                PortType.Serial => MaterialIconKind.SerialPort,
                PortType.Udp => MaterialIconKind.IpNetworkOutline,
                PortType.Tcp => MaterialIconKind.RouterWireless,
                _ => throw new ArgumentOutOfRangeException(nameof(infoType), infoType, null)
            };
        }

        [Reactive]
        public string Name { get; set; }

        [Reactive]
        public MaterialIconKind Icon { get; set; }

        [Reactive]
        public string RxText { get; set; }
        [Reactive]
        public string TxText { get; set; }

        [Reactive]
        public string ConnectionString { get; set; }

        [Reactive]
        public bool IsPortEnabled { get; set; }

        public ICommand EnableDisableCommand { get; }

        [Reactive]
        public bool IsConnected { get; set; }
        [Reactive]
        public bool IsError { get; set; }
        [Reactive]
        public bool IsDisabled { get; set; }
        [Reactive]
        public bool IsConnecting { get; set; }

        public ICommand DeletePortCommand { get; }

        [Reactive]
        public string SkippedText { get; set; }

        [Reactive]
        public string Description { get; set; }

        [Reactive]
        public string? Error { get; set; }
    }
}