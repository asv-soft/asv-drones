﻿using System;
using System.Composition;
using System.Net;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Avalonia.Controls;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui
{
    public class UdpPortViewModel : ViewModelBaseWithValidation
    {
        private readonly IMavlinkDevicesService? _device;


        public UdpPortViewModel() : base(new Uri(WellKnownUri.ShellPageSettingsConnectionsPortsUri, "ports.udp"))
        {
            LocalPortString = "";
            RemotePortString = "";
            PacketLossChance = 0;
#if DEBUG
            IsDebugMode = true;
#endif
            if (Design.IsDesignMode)
            {
            }
            
            this.WhenValueChanged(x => x.LocalPort).Subscribe(v =>
            {
                if (v != null) LocalPortString = v.ToString();
            }).DisposeItWith(Disposable);
            this.WhenValueChanged(x => x.RemotePort).Subscribe(v =>
            {
                if (v != null) RemotePortString = v.ToString();
            }).DisposeItWith(Disposable);
            this.WhenValueChanged(x => x.LocalPortString).Subscribe(v =>
            {
                if (!string.IsNullOrWhiteSpace(v) & int.TryParse(v, out var port)) LocalPort = port;
            }).DisposeItWith(Disposable);
            this.WhenValueChanged(x => x.RemotePortString).Subscribe(v =>
            {
                if (!string.IsNullOrWhiteSpace(v) & int.TryParse(v, out var port)) RemotePort = port;
            }).DisposeItWith(Disposable);
            this.WhenValueChanged(x => x.PacketLossChanceString).Subscribe(v =>
            {
                if (v is not null && int.TryParse(v, out int chance)) PacketLossChance = chance;
            }).DisposeItWith(Disposable);
            this.WhenValueChanged(x => x.PacketLossChance).Subscribe(v =>
            {
                if (v is not null) PacketLossChanceString = PacketLossChance.ToString();
            }).DisposeItWith(Disposable);
            this.WhenValueChanged(_ => IsRemote).Subscribe(_ => UpdateValidationRules()).DisposeItWith(Disposable);
        }

        private void UpdateValidationRules()
        {
            this.ClearValidationRules();

            this.ValidationRule(x => x.LocalPortString, localPortStr =>
            {
                var isInt = int.TryParse(localPortStr, out int port);

                if (!isInt)
                {
                    return false;
                }

                return port is > 1 and < 65535;
            }, RS.UdpPortViewModel_ValidLocalPort).DisposeItWith(Disposable);
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
            
            this.ValidationRule(x => x.Title, title => !string.IsNullOrWhiteSpace(title), RS.UdpPortViewModel_ValidTitle)
                .DisposeItWith(Disposable);
            this.ValidationRule(x => x.LocalIpAddress,
                    ip => !string.IsNullOrWhiteSpace(ip) && IPAddress.TryParse(ip, out IPAddress? _),
                    RS.UdpPortViewModel_ValidLocalIpAddress)
                .DisposeItWith(Disposable);
            
            this.ValidationRule(x => x.PacketLossChance, chance => chance is not null,
                    RS.PortViewModel_ValidPacketLossChance)
                .DisposeItWith(Disposable);

            if (IsRemote)
            {
                this.ValidationRule(x => x.RemoteIpAddress,
                        ip => !string.IsNullOrWhiteSpace(ip) && IPAddress.TryParse(ip, out IPAddress? _),
                        RS.UdpPortViewModel_ValidRemoteIpAddress)
                    .DisposeItWith(Disposable);
                this.ValidationRule(x => x.RemotePortString, portStr =>
                    {
                        var isInt = int.TryParse(portStr, out int port);

                        if (!isInt)
                        {
                            return false;
                        }
                        
                        return port is > 1 and < 65535;
                    }, RS.UdpPortViewModel_ValidRemotePort)
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
                    ConnectionString = $"udp://{LocalIpAddress}:{LocalPort}" +
                                       (IsRemote ? $"?rhost={RemoteIpAddress}&rport={RemotePort}" : string.Empty),
                    IsEnabled = true,
                    PacketLossChance = PacketLossChance ?? 0,
                });
            }
            catch (Exception e)
            {
            }
        }

        [Reactive] public string Title { get; set; }
        [Reactive] public int? LocalPort { get; set; }
        [Reactive] public string LocalIpAddress { get; set; }
        [Reactive] public string LocalPortString { get; set; }
        [Reactive] public bool IsRemote { get; set; }
        [Reactive] public string RemoteIpAddress { get; set; }
        [Reactive] public int? RemotePort { get; set; }
        [Reactive] public string RemotePortString { get; set; }
        
        [Reactive] public int? PacketLossChance { get; set; } = 0;
        
        [Reactive] public string PacketLossChanceString { get; set; }
        
        public bool IsDebugMode { get; private set; }
    }
}