using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Uav;
using Asv.IO;
using Avalonia.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    public class PortViewModel:ViewModelBase
    {
        private readonly IMavlinkDevicesService _svc;
        private readonly ILocalizationService _localization;
        private readonly Guid _id;
        private readonly IncrementalRateCounter _rxByteRate = new(3);
        private readonly IncrementalRateCounter _rxPacketRate = new(3);
        private readonly IncrementalRateCounter _txByteRate = new(3);
        private readonly IncrementalRateCounter _txPacketRate = new(3);


        public PortViewModel() : base(new Uri(ConnectionsViewModel.BaseUri, $"port.{Guid.NewGuid()}"))
        {
            if (Design.IsDesignMode)
            {
                PortId = Guid.NewGuid();
                Icon = ConvertIcon(PortType.Serial);
                RxText = "0.456";
                RxUnitText = "kb/s";
                RxPktText = "156";
                RxPktUnitText = "Hz";
                
                    
                TxText = "0.456";
                TxUnitText = "kb/s";
                TxPktText = "156";
                TxPktUnitText = "Hz";

                SkippedText = "153";
                SkippedUnitText = "pkt";
                ConnectionString = "tcp://127.0.0.1:7341?srv=true";
            }
            
        }

        public PortViewModel(Guid id):base(new Uri(ConnectionsViewModel.BaseUri, $"port.{id}"))
        {
            PortId = id;
            
        }

        public PortViewModel(IMavlinkDevicesService svc, ILocalizationService localization, Guid id):this(id)
        {
            _svc = svc ?? throw new ArgumentNullException(nameof(svc));
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
            _id = id;
            Observable.Timer(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(1)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(Update).DisposeItWith(Disposable);
            EnableDisableCommand = ReactiveCommand.Create(() => _svc.Router.SetEnabled(_id, IsPortEnabled)).DisposeItWith(Disposable);
            DeletePortCommand = ReactiveCommand.Create(() => _svc.Router.RemovePort(_id)).DisposeItWith(Disposable);
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

            var rxByteRate = _rxByteRate.Calculate(info.RxBytes);
            RxText = _localization.ByteRate.ConvertToString(rxByteRate);
            RxUnitText = _localization.ByteRate.GetUnit(rxByteRate);

            var rxPktRate = _rxPacketRate.Calculate(info.RxPackets);
            RxPktText = _localization.ItemsRate.ConvertToString(rxPktRate);
            RxPktUnitText = _localization.ItemsRate.GetUnit(rxPktRate);

            var txByteRate = _txByteRate.Calculate(info.TxBytes);
            TxText = _localization.ByteRate.ConvertToString(txByteRate);
            TxUnitText = _localization.ByteRate.GetUnit(txByteRate);

            var txPktRate = _txPacketRate.Calculate(info.TxPackets);
            TxPktText = _localization.ItemsRate.ConvertToString(txPktRate);
            TxPktUnitText = _localization.ItemsRate.GetUnit(txPktRate);

            IsPortEnabled = info.IsEnabled ?? false;
            SkippedText = $"{info.SkipPackets}";
            SkippedUnitText = RS.PortViewModel_SkippedUnitTest;

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
        public string RxUnitText { get; set; }
        [Reactive]
        public string RxPktUnitText { get; set; }
        [Reactive]
        public string RxPktText { get; set; }

        
        
        [Reactive]
        public string TxText { get; set; }
        [Reactive]
        public string TxUnitText { get; set; }
        [Reactive]
        public string TxPktUnitText { get; set; }
        [Reactive]
        public string TxPktText { get; set; }


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
        public string SkippedUnitText { get; set; }

        [Reactive]
        public string Description { get; set; }

        [Reactive]
        public string? Error { get; set; }
        
    }
}