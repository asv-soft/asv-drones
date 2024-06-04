using System;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Api;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui;

public class SourceViewModel : DisposableReactiveObjectWithValidation
{
    private readonly IPluginManager _mng;
    private readonly ILogService _log;
    private readonly PluginSourceViewModel? _viewModel;

    public SourceViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        this.ValidationRule(x => x.Name, _ => !string.IsNullOrWhiteSpace(_),
                RS.SourceViewModel_SourceViewModel_Name_is_required)
            .DisposeItWith(Disposable);

        this.ValidationRule(x => x.SourceUri, _ => !string.IsNullOrWhiteSpace(_),
                RS.SourceViewModel_SourceViewModel_SourceUri_is_required)
            .DisposeItWith(Disposable);
    }

    public SourceViewModel(IPluginManager mng, ILogService log, PluginSourceViewModel? viewModel)
    {
        _mng = mng;
        _log = log;
        _viewModel = viewModel;
        this.ValidationRule(x => x.Name, x => !string.IsNullOrWhiteSpace(x),
                RS.SourceViewModel_SourceViewModel_Name_is_required)
            .DisposeItWith(Disposable);
        this.ValidationRule(x => x.SourceUri, x => !string.IsNullOrWhiteSpace(x),
                RS.SourceViewModel_SourceViewModel_SourceUri_is_required)
            .DisposeItWith(Disposable);
        if (_viewModel != null)
        {
            Name = _viewModel.Name;
            SourceUri = _viewModel.SourceUri;
            Username = _viewModel.Model.Username;
        }
    }

    [Reactive] public string Name { get; set; }
    [Reactive] public string SourceUri { get; set; }
    [Reactive] public string? Username { get; set; }
    [Reactive] public string Password { get; set; }


    public void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);
        dialog.PrimaryButtonCommand =
            ReactiveCommand.Create(Update, this.IsValid().Do(x => dialog.IsPrimaryButtonEnabled = x))
                .DisposeItWith(Disposable);
    }

    private void Update()
    {
        if (_viewModel == null)
        {
            _mng.AddServer(new PluginServer(Name, SourceUri, Username, Password));
        }
        else
        {
            _mng.RemoveServer(_viewModel.Model);
            _mng.AddServer(new PluginServer(Name, SourceUri, Username, Password));
        }
    }
}