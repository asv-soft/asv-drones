using System;
using System.Composition;
using System.Net;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui
{
    public class TcpPortViewModel : ViewModelBaseWithValidation
    {
        private readonly IMavlinkDevicesService? _deviceService;
        private readonly ILogService _logService;

        public TcpPortViewModel() : base(new Uri(WellKnownUri.ShellPageSettingsConnectionsPortsUri, "ports.tcp"))
        {
        }

        [ImportingConstructor]
        public TcpPortViewModel(IMavlinkDevicesService deviceService, ILogService logService) : this()
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            Title = $"{RS.TcpPortViewModel_Title}: {deviceService.Router.GetPorts().Length}";
            Port = 7334;
            IpAddress = "172.16.0.1";
            IsTcpIpServer = true;
            PacketLossChance = 0;
#if DEBUG
            IsDebugMode = true;
#endif
            
            this.WhenValueChanged(x => x.PortString).Subscribe(v =>
            {
                if (v != null & int.TryParse(v, out int port)) Port = port;
            }).DisposeItWith(Disposable);
            this.WhenValueChanged(x => x.PacketLossChanceString).Subscribe(v =>
            {
                if (v is not null && int.TryParse(v, out int chance)) PacketLossChance = chance;
            }).DisposeItWith(Disposable);
            this.WhenValueChanged(x => x.Port).Subscribe(v =>
            {
                if (v != null) PortString = Port.ToString();
            }).DisposeItWith(Disposable);
            this.WhenValueChanged(x => x.PacketLossChance).Subscribe(v =>
            {
                if (v is not null) PacketLossChanceString = PacketLossChance.ToString();
            }).DisposeItWith(Disposable);
            
            this.ValidationRule(x => x.Title, title => !string.IsNullOrWhiteSpace(title), 
                    RS.TcpPortViewModel_ValidTitle)
                .DisposeItWith(Disposable);
            this.ValidationRule(x => x.IpAddress,
                    ipStr => !string.IsNullOrWhiteSpace(ipStr) && IPAddress.TryParse(ipStr, out IPAddress? ip),
                    RS.TcpPortViewModel_ValidIpAddress)
                .DisposeItWith(Disposable);
            this.ValidationRule(x => x.PortString,
                    portStr =>
                    {
                        var isInt = int.TryParse(portStr, out int port);

                        if (!isInt)
                        {
                            return false;
                        }

                        return port is > 1 and < 65535;
                    },
                    RS.TcpPortViewModel_ValidPort)
                .DisposeItWith(Disposable);
            this.ValidationRule(x => x.PacketLossChanceString, chanceStr =>
                    {
                        var isInt = int.TryParse(chanceStr, out int chance);

                        if (!isInt)
                        {
                            return false;
                        }
                        
                        return chance is <= 100 and >= 0;
                    },
                    RS.PortViewModel_ValidPacketLossChance)
                .DisposeItWith(Disposable);
            
            this.ValidationRule(x => x.PacketLossChance, chance => chance is not null,
                    RS.PortViewModel_ValidPacketLossChance)
                .DisposeItWith(Disposable);
            this.ValidationRule(x => x.Port, port => port is not null, 
                    RS.TcpPortViewModel_ValidPort)
                .DisposeItWith(Disposable);
        }


        public void ApplyDialog(ContentDialog dialog)
        {
            if (dialog == null) throw new ArgumentNullException(nameof(dialog));
            dialog.PrimaryButtonCommand =
                ReactiveCommand.Create(AddTcpPort, this.IsValid().Do(_ => dialog.IsPrimaryButtonEnabled = _))
                    .DisposeItWith(Disposable);
        }

        private void AddTcpPort()
        {
            // this is for design mode
            if (_deviceService == null) return;
            if (Port is { } port)
            {
                try
                {
                    _deviceService.Router.AddPort(new MavlinkPortConfig
                    {
                        Name = Title,
                        ConnectionString = $"tcp://{IpAddress}:{port}" + (IsTcpIpServer ? "?srv=true" : string.Empty),
                        IsEnabled = true,
                        PacketLossChance = PacketLossChance ?? 0,
                    });
                }
                catch (Exception e)
                {
                    _logService.Error("", $"{RS.TcpPortViewModel_LogService_Error}:{e.Message}", e);
                }
            }
            else
            {
                _logService.Error("", $"{RS.TcpPortViewModel_LogService_Error}");
            }
        }

        [Reactive] public string Title { get; set; }

        [Reactive] public int? Port { get; set; }
        [Reactive] public string PortString { get; set; }

        [Reactive] public bool IsTcpIpServer { get; set; }

        [Reactive] public string IpAddress { get; set; }

        [Reactive] public int? PacketLossChance { get; set; } = 0;
        
        [Reactive] public string PacketLossChanceString { get; set; }
        
        public bool IsDebugMode { get; private set; }
    }
}