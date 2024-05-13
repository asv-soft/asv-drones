using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class PluginsSourcesViewModel : TreePageViewModel
{
    private readonly ISettingsPageContext _context;
    private readonly IPluginManager _mng;
    private readonly ILogService _log;
    private readonly ReadOnlyObservableCollection<PluginSourceViewModel> _items;

    public PluginsSourcesViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
        Update = ReactiveCommand.Create(() => { });
        _items = new ReadOnlyObservableCollection<PluginSourceViewModel>(
            new ObservableCollection<PluginSourceViewModel>(
                new[]
                {
                    new PluginSourceViewModel
                    {
                        Id = "#1",
                        Name = "Nuget",
                        SourceUri = "https://api.nuget.org/v3/index.json"
                    },
                    new PluginSourceViewModel
                    {
                        Id = "#2",
                        Name = "Github",
                        SourceUri = "https://api.github.com/repos/Asv.Drones.Gui.Plugins/releases"
                    }
                }));
    }

    public PluginsSourcesViewModel(ISettingsPageContext context, IPluginManager mng, ILogService log) : base(
        WellKnownUri.ShellPageSettingsPluginsSourceUri)
    {
        _context = context;
        _mng = mng;
        _log = log;

        var sourceCache = new SourceCache<IPluginServerInfo, string>(x => x.Name).DisposeItWith(Disposable);


        Update = ReactiveCommand.Create(() =>
        {
            sourceCache.Clear();
            sourceCache.AddOrUpdate(mng.Servers);
        }).DisposeItWith(Disposable);
        ;
        Update.ThrownExceptions.Subscribe(ex =>
        {
            log.Error(Title, RS.PluginsSourcesViewModel_PluginsSourcesViewModel_Error_to_update, ex);
        }).DisposeItWith(Disposable);
        Update.Execute().Subscribe();

        Remove = ReactiveCommand.Create<PluginSourceViewModel>(x =>
        {
            mng.RemoveServer(x.Model);
            Update.Execute().Subscribe();
        }).DisposeItWith(Disposable);
        ;
        Remove.ThrownExceptions.Subscribe(ex =>
        {
            log.Error(Title, RS.PluginsSourcesViewModel_PluginsSourcesViewModel_Error_to_remove, ex);
        }).DisposeItWith(Disposable);

        Actions = new ReadOnlyObservableCollection<IMenuItem>([
            new MenuItem($"{Id}.action.add")
            {
                Header = RS.PluginsSourcesViewModel_AddAction_Label,
                Icon = MaterialIconKind.WebPlus,
                Command = ReactiveCommand.CreateFromTask(AddImpl).DisposeItWith(Disposable)
            }
        ]);

        Edit = ReactiveCommand.CreateFromTask<PluginSourceViewModel, Unit>(EditImpl).DisposeItWith(Disposable);

        sourceCache
            .Connect()
            .Transform(x => new PluginSourceViewModel(x, this))
            .SortBy(x => x.Name)
            .Bind(out _items)
            .Subscribe()
            .DisposeItWith(Disposable);
    }

    public string Title => "Servers";

    public ReactiveCommand<Unit, Unit> Update { get; }
    public ReactiveCommand<PluginSourceViewModel, Unit> Remove { get; }
    public ReactiveCommand<PluginSourceViewModel, Unit> Edit { get; }
    public ReactiveCommand<Unit, Unit> Add { get; }

    private async Task AddImpl(CancellationToken cancel)
    {
        var dialog = new ContentDialog()
        {
            Title = RS.PluginsSourcesViewModel_AddImpl_Title,
            PrimaryButtonText = RS.PluginsSourcesViewModel_AddImpl_Add,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = RS.PluginsSourcesViewModel_AddImpl_Cancel
        };
        using var viewModel = new SourceViewModel(_mng, _log, null);
        viewModel.ApplyDialog(dialog);
        dialog.Content = viewModel;
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            Update.Execute().Subscribe();
        }
    }

    private async Task<Unit> EditImpl(PluginSourceViewModel arg, CancellationToken cancel)
    {
        var dialog = new ContentDialog()
        {
            Title = RS.PluginsSourcesViewModel_EditImpl_Title,
            PrimaryButtonText = RS.PluginsSourcesViewModel_EditImpl_Save,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = RS.PluginsSourcesViewModel_AddImpl_Cancel
        };
        using var viewModel = new SourceViewModel(_mng, _log, arg);
        viewModel.ApplyDialog(dialog);
        dialog.Content = viewModel;
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            Update.Execute().Subscribe();
        }

        return Unit.Default;
    }

    public ReadOnlyObservableCollection<PluginSourceViewModel> Items => _items;
    [Reactive] public PluginSourceViewModel SelectedItem { get; set; }
}