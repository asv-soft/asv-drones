using Asv.Drones.Gui.Core;
using FluentAvalonia.UI.Controls;
using ReactiveUI.Validation.Extensions;
using ReactiveUI;
using System.ComponentModel.Composition;
using System.Net;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav
{
    public class TcpPortViewModel:ViewModelBaseWithValidation
    {
        private readonly IMavlinkDevicesService? _deviceService;
        private readonly ILogService _logService;

        public TcpPortViewModel() : base(new Uri(ConnectionsViewModel.BaseUri, "ports.tcp"))
        {

        }

        [ImportingConstructor]
        public TcpPortViewModel(IMavlinkDevicesService deviceService,ILogService logService) : this()
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            // TODO: Localize
            Title = $"New TCP {deviceService.Router.GetPorts().Length}";
            Port = 7341;
            IpAddress = "172.16.0.1";
            IsTcpIpServer = true;

            this.ValidationRule(x => x.Title, _ => !string.IsNullOrWhiteSpace(_), "You must specify a valid name") // TODO: Localize
                .DisposeItWith(Disposable);

            this.ValidationRule(x => x.Port, _ => _ is > 1 and < 65535, "Port number must be value from 1 to 65535") // TODO: Localize
                .DisposeItWith(Disposable);

            this.ValidationRule(x => x.IpAddress, _ => !string.IsNullOrWhiteSpace(_) && IPAddress.TryParse(_, out IPAddress? ip), "You must insert a valid ip address") // TODO: Localize
                .DisposeItWith(Disposable);
            
        }


        public void ApplyDialog(ContentDialog dialog)
        {
            if (dialog == null) throw new ArgumentNullException(nameof(dialog));
            dialog.PrimaryButtonCommand =
                ReactiveCommand.Create(AddTcpPort, this.IsValid().Do(_ => dialog.IsPrimaryButtonEnabled = _)).DisposeItWith(Disposable);
        }

        private void AddTcpPort()
        {
            // this is for design mode
            if (_deviceService == null) return;

            try
            {
                _deviceService.Router.AddPort(new MavlinkPortConfig
                {
                    Name = Title,
                    ConnectionString = $"tcp://{IpAddress}:{Port}" + (IsTcpIpServer ? "?srv=true":string.Empty),
                    IsEnabled = true,
                });
                
            }
            catch (Exception e)
            {
                _logService.Error("", $"Error to create port:{e.Message}", e); // TODO: Localize
            }
        }

        [Reactive]
        public string Title { get; set; }

        [Reactive] 
        public int Port { get; set; }

        [Reactive]
        public bool IsTcpIpServer { get; set; }

        [Reactive]
        public string IpAddress { get; set; }
    }
}