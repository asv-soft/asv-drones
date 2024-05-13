using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui;

public class PortBrowserViewModel : TreePageViewModel
{
    private readonly IMavlinkDevicesService _deviceSvc;
    private readonly ILogService _logService;
    private readonly ReadOnlyObservableCollection<PortViewModel> _items;

    public PortBrowserViewModel() : base(WellKnownUri.Undefined)
    {
        DesignTime.ThrowIfNotDesignMode();
        var portList = new List<PortViewModel>
        {
            new() { Name = "TCP port" },
            new() { Name = "UDP port" },
            new() { Name = "Serial port" },
        };
        _items = new ReadOnlyObservableCollection<PortViewModel>(new ObservableCollection<PortViewModel>(portList));
    }

    [ImportingConstructor]
    public PortBrowserViewModel(IMavlinkDevicesService deviceSvc, ILogService logService,
        ILocalizationService localization)
        : base(WellKnownUri.ShellPageSettingsConnectionsPortsUri)
    {
        _deviceSvc = deviceSvc ?? throw new ArgumentNullException(nameof(deviceSvc));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        var cache = new SourceCache<PortViewModel, Guid>(_ => _.PortId).DisposeItWith(Disposable);
        cache.Connect()
            .Bind(out _items)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        deviceSvc.Router
            .GetPorts()
            .ForEach(_ => cache.AddOrUpdate(new PortViewModel(deviceSvc, localization, _logService, _)));
        deviceSvc.Router
            .OnAddPort
            .Subscribe(_ => cache.AddOrUpdate(new PortViewModel(deviceSvc, localization, _logService, _)))
            .DisposeItWith(Disposable);
        deviceSvc.Router
            .OnRemovePort
            .Subscribe(_ => cache.Remove(_)).DisposeItWith(Disposable);

        Actions = new ReadOnlyObservableCollection<IMenuItem>([
            new MenuItem($"{Id}.action.add-serial")
            {
                Header = RS.ConnectionsPortsView_AddSerialCommand_Title,
                Icon = MaterialIconKind.SerialPort,
                Command = ReactiveCommand.CreateFromTask(AddSerialPort).DisposeItWith(Disposable)
            },
            new MenuItem($"{Id}.action.add-tcp")
            {
                Header = RS.ConnectionsPortsView_AddTcpCommand_Title,
                Icon = MaterialIconKind.RouterWireless,
                Command = ReactiveCommand.CreateFromTask(AddTcpPort).DisposeItWith(Disposable)
            },
            new MenuItem($"{Id}.action.add-udp")
            {
                Header = RS.ConnectionsPortsView_AddUdpCommand_Title,
                Icon = MaterialIconKind.IpNetworkOutline,
                Command = ReactiveCommand.CreateFromTask(AddUdpPort).DisposeItWith(Disposable)
            },
        ]);

        /*Actions = new ReadOnlyObservableCollection<IMenuItem>([
            new MenuItem($"{WellKnownUri.ShellPageSettingsConnectionsPorts}.serial")
            {
                Header = RS.ConnectionsPortsView_AddSerialCommand_Title,
                Command = AddSerialPortCommand,
            },
            new MenuItem($"{WellKnownUri.ShellPageSettingsConnectionsPorts}.tcp")
            {
                Header = RS.ConnectionsPortsView_AddTcpCommand_Title,
                Command = AddTcpPortCommand,
            },
            new MenuItem($"{WellKnownUri.ShellPageSettingsConnectionsPorts}.udp")
            {
                Header = RS.ConnectionsPortsView_AddUdpCommand_Title,
                Command = AddUdpPortCommand,
            }

        ]);*/
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
        using var viewModel = new SerialPortViewModel(_deviceSvc, _logService);
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
        using var viewModel = new TcpPortViewModel(_deviceSvc, _logService);
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
        using var viewModel = new UdpPortViewModel(_deviceSvc);
        viewModel.ApplyDialog(dialog);
        dialog.Content = viewModel;
        var result = await dialog.ShowAsync();
    }

    public ReadOnlyObservableCollection<PortViewModel> Items => _items;
    public bool IsRebootRequired { get; private set; }
    public int Order => 0;
}