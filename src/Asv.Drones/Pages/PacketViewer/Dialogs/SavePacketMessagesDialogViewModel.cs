using System;
using System.Collections.Generic;
using System.Composition;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class SavePacketMessagesDialogViewModel : DialogViewModelBase
{
    public const string ViewModelId = $"{PacketViewerViewModel.PageId}.{BaseId}.separator";
    public const string DefaultSeparator = ";";
    public const string DefaultShieldSymbol = ",";

    public SavePacketMessagesDialogViewModel()
        : this(DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public SavePacketMessagesDialogViewModel(ILoggerFactory loggerFactory)
        : base(ViewModelId, loggerFactory)
    {
        IsSemicolon = new BindableReactiveProperty<bool>(true).DisposeItWith(Disposable);
        IsComa = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        IsTab = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        Separator = DefaultSeparator;
        ShieldSymbol = DefaultShieldSymbol;

        _sub2 = IsSemicolon
            .Where(value => value)
            .Subscribe(_ =>
            {
                Separator = DefaultSeparator;
                ShieldSymbol = DefaultShieldSymbol;
            });

        _sub3 = IsComa
            .Where(value => value)
            .Subscribe(_ =>
            {
                Separator = ",";
                ShieldSymbol = ";";
            });

        _sub4 = IsTab
            .Where(value => value)
            .Subscribe(_ =>
            {
                Separator = "\t";
                ShieldSymbol = DefaultShieldSymbol;
            });
    }

    public BindableReactiveProperty<bool> IsSemicolon { get; }
    public BindableReactiveProperty<bool> IsComa { get; }
    public BindableReactiveProperty<bool> IsTab { get; }

    private string _separator;
    public string Separator
    {
        get => _separator;
        set => SetField(ref _separator, value);
    }

    private string _shieldSymbol;
    public string ShieldSymbol
    {
        get => _shieldSymbol;
        set => SetField(ref _shieldSymbol, value);
    }

    public override void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);

        _sub5 = IsValid.Subscribe(isValid =>
        {
            dialog.IsPrimaryButtonEnabled = isValid;
        });
    }

    public override IEnumerable<IRoutable> GetRoutableChildren() => [];

    #region Dispose

    private readonly IDisposable _sub2;
    private readonly IDisposable _sub3;
    private readonly IDisposable _sub4;
    private IDisposable _sub5;

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            _sub2.Dispose();
            _sub3.Dispose();
            _sub4.Dispose();
            _sub5.Dispose();
        }

        base.Dispose(isDisposing);
    }

    #endregion
}
