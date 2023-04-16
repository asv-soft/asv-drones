using System.ComponentModel.Composition;
using System.Net;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using Avalonia.Controls;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Core
{
    public class UdpPortViewModel:ViewModelBaseWithValidation
    {
        private readonly IMavlinkDevicesService? _device;
        

        public UdpPortViewModel() : base(new Uri(ConnectionsViewModel.BaseUri, "ports.udp"))
        {
            if (Design.IsDesignMode)
            {
        
            }

            this.WhenValueChanged(_ => IsRemote).Subscribe(_ => UpdateValidationRules()).DisposeItWith(Disposable);
        }

        private void UpdateValidationRules()
        {
            this.ClearValidationRules();
            
            this.ValidationRule(x => x.Title, _ => !string.IsNullOrWhiteSpace(_), RS.UdpPortViewModel_ValidTitle)
                .DisposeItWith(Disposable);

            this.ValidationRule(x => x.LocalIpAddress, _ => !string.IsNullOrWhiteSpace(_) && IPAddress.TryParse(_, out IPAddress? ip), RS.UdpPortViewModel_ValidLocalIpAddress)
                .DisposeItWith(Disposable);

            this.ValidationRule(x => x.LocalPort, _ => _ is > 1 and < 65535, RS.UdpPortViewModel_ValidLocalPort)
                .DisposeItWith(Disposable);

            if (IsRemote)
            {
                this.ValidationRule(x => x.RemoteIpAddress, _ => !string.IsNullOrWhiteSpace(_) && IPAddress.TryParse(_, out IPAddress? ip), RS.UdpPortViewModel_ValidRemoteIpAddress)
                    .DisposeItWith(Disposable);
                this.ValidationRule(x => x.RemotePort, _ => _ is > 1 and < 65535, RS.UdpPortViewModel_ValidRemotePort)
                    .DisposeItWith(Disposable);
                
            }
        }

        [ImportingConstructor]
        public UdpPortViewModel(IMavlinkDevicesService device) : this()
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            Title = $"{RS.UdpPortViewModel_Title} {device.Router.GetPorts().Length}";
        }

        public void ApplyDialog(ContentDialog dialog)
        {
            if (dialog == null) throw new ArgumentNullException(nameof(dialog));
            dialog.PrimaryButtonCommand =
                ReactiveCommand.Create(AddUdpPort, this.IsValid().Do(_ => dialog.IsPrimaryButtonEnabled = _))
                    .DisposeItWith(Disposable);
        }

        private void AddUdpPort()
        {
            if (_device == null) return;
            try
            {
                _device.Router.AddPort(new MavlinkPortConfig
                {
                    Name = Title,
                    ConnectionString = $"udp://{LocalIpAddress}:{LocalPort}" + (IsRemote ? $"?rhost={RemoteIpAddress}&rport={RemotePort}":string.Empty),
                    IsEnabled = false
                });
            }
            catch (Exception e)
            {
            }
        }

        [Reactive]
        public string Title { get; set; }
        [Reactive]
        public int LocalPort { get; set; }
        [Reactive]
        public string LocalIpAddress { get; set; }

        [Reactive]
        public bool IsRemote { get; set; }
        [Reactive]
        public string RemoteIpAddress { get; set; }
        [Reactive]
        public int RemotePort { get; set; }
    }
}
