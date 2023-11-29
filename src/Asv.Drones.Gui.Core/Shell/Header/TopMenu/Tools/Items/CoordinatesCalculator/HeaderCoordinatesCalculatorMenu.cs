using System.ComponentModel.Composition;
using Asv.Cfg;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Core;

[Export(HeaderMenuItem.UriString + "/tools", typeof(IHeaderMenuItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class HeaderCoordinatesCalculatorMenu : HeaderMenuItem
{
    public const string UriString = HeaderMenuItem.UriString + "tools/coord-calc";
    public static readonly Uri Uri = new(UriString);
    private readonly ILocalizationService _loc;
    private readonly IConfiguration _cfg;

    [ImportingConstructor]
    public HeaderCoordinatesCalculatorMenu(ILocalizationService loc, IConfiguration cfg):base(Uri)
    {
        _loc = loc ?? throw new ArgumentNullException(nameof(loc));
        _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        Header = RS.HeaderCoordinatesCalculatorMenu_Header;
        Icon = MaterialIconKind.DatabaseOutline;
        Order = 0;
        //Command = cmd.OpenStore;
        HotKey = KeyGesture.Parse("Alt+C");
        Command = ReactiveCommand.CreateFromTask(ExecuteImpl);
    }
    
    private async Task ExecuteImpl(CancellationToken cancel)
    {
        var dialog = new ContentDialog
        {
            Title = RS.CoordinatesCalculatorDialog_Title,
            CloseButtonText = RS.CoordinatesCalculatorDialog_CloseButton,
            IsPrimaryButtonEnabled = false,
            IsSecondaryButtonEnabled = false,
            FullSizeDesired = true
        };
        
        using var viewModel = new CoordinatesCalculatorViewModel(_cfg, _loc);
        viewModel.ApplyDialog(dialog);
        dialog.Content = viewModel;
            
        var result = await dialog.ShowAsync();
    }
}