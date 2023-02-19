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
        private readonly IMavlinkDevicesService? _device;

        public TcpPortViewModel() : base(new Uri(ConnectionsViewModel.BaseUri, "ports.tcp"))
        {

        }

        [ImportingConstructor]
        public TcpPortViewModel(IMavlinkDevicesService device) : this()
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            Title = "New TCP " + device.Router.GetPorts().Length;

            this.ValidationRule(x => x.Title, _ => !string.IsNullOrWhiteSpace(_), "You must specify a valid name")
                .DisposeItWith(Disposable);

            this.ValidationRule(x => x.Port, _ => _ is > 1 and < 65535, "Port number must be value from 1 to 65535")
                .DisposeItWith(Disposable);
            this.ValidationRule(x => x.ServerIp, _ => !string.IsNullOrWhiteSpace(_) && IPAddress.TryParse(_, out IPAddress? ip), "You must insert a valid ip address")
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
            if (_device == null) return;
            try
            {
                _device.Router.AddPort(new MavlinkPortConfig
                {
                    Name = Title,
                    ConnectionString = $"tcp://{ServerIp}:{Port}" + (IsServer ? "?srv=true":string.Empty),
                    IsEnabled = true,
                });
                
            }
            catch (Exception e)
            {

            }
        }

        [Reactive]
        public string Title { get; set; }

        [Reactive]
        public int Port { get; set; }

        [Reactive]
        public bool IsServer { get; set; }

        [Reactive]
        public string ServerIp { get; set; }
    }
}