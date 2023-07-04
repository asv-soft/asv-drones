using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO.Ports;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using DynamicData;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Core
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SerialPortViewModel: ViewModelBaseWithValidation
    {
        private readonly IMavlinkDevicesService? _device;
        private readonly ILogService _logService;
        private int _requestNotComplete;
        private readonly ReadOnlyObservableCollection<string> _ports;
        private readonly SourceList<string> _myCache;
        public const int MinBaudRate = 75;
        public const int MaxBaudRate = 921_600;

        public SerialPortViewModel() : base(new Uri(ConnectionsViewModel.BaseUri,"ports.serial"))
        {
            
            this.ValidationRule(x => x.Title, _ => !string.IsNullOrWhiteSpace(_), RS.SerialPortViewModel_SerialPortViewModel_You_must_specify_a_valid_name)
                .DisposeItWith(Disposable);

            this.ValidationRule(x => x.SelectedPort, _ => !string.IsNullOrWhiteSpace(_), RS.SerialPortViewModel_SerialPortViewModel_ValidSerialPort)
                .DisposeItWith(Disposable);

            this
                .ValidationRule(x => x.SelectedBaudRate, _ => _ is >= MinBaudRate and <= MaxBaudRate, string.Format(RS.SerialPortViewModel_SerialPortViewModel_BaudRateValid, MinBaudRate, MaxBaudRate))
                .DisposeItWith(Disposable);

            _myCache = new SourceList<string>()
                .DisposeItWith(Disposable);
            _myCache.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _ports)
                .Subscribe(_ => SelectedPort ??= _ports.FirstOrDefault())
                .DisposeItWith(Disposable);
            Observable
                .Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1))
                .Subscribe(UpdateSerialPorts)
                .DisposeItWith(Disposable);
        }

        [ImportingConstructor]
        public SerialPortViewModel(IMavlinkDevicesService device,ILogService logService):this()
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _logService = logService;
            Title = string.Format(RS.SerialPortViewModel_SerialPortViewModel_NewSerial, device.Router.GetPorts().Length);
        }

        public void ApplyDialog(ContentDialog dialog)
        {
            if (dialog == null) throw new ArgumentNullException(nameof(dialog));
            dialog.PrimaryButtonCommand =
                ReactiveCommand.Create(AddSerialPort, this.IsValid().Do(_ =>dialog.IsPrimaryButtonEnabled = _)).DisposeItWith(Disposable);
        }

        private void AddSerialPort()
        {
            if (_device == null) return;
            try
            {
                _device.Router.AddPort(new MavlinkPortConfig
                {
                    Name = Title,
                    ConnectionString = $"serial:{SelectedPort}?br={SelectedBaudRate}",
                    IsEnabled = true
                });
            }
            catch (Exception e)
            {
                _logService.Error("",$"Error to create port:{e.Message}",e);
            }
        }

        private void UpdateSerialPorts(long l)
        {
            if (Interlocked.CompareExchange(ref _requestNotComplete, 1, 0) != 0) return;
            try
            {
                var value = SerialPort.GetPortNames();
                var exist = _myCache.Items.ToArray();
                var toDelete = exist.Except(value).ToArray();
                var toAdd = value.Except(exist).ToArray();
                _myCache.RemoveMany(toDelete);
                _myCache.AddRange(toAdd);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                Interlocked.Exchange(ref _requestNotComplete, 0);
            }
        }

        public ReadOnlyObservableCollection<string> Ports => _ports;

        public ReadOnlyObservableCollection<int> BaudRates { get; } = new(
            new ObservableCollection<int>(new[] { 9600, 14400, 19200, 38400, 56000, 57600, 115200, 128000, 256000 }));

        [Reactive]
        public string Title { get; set; }

        [Reactive] public int SelectedBaudRate { get; set; } = 115_200;

        [Reactive]
        public string SelectedPort { get; set; }
    }
}