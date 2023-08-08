using System.ComponentModel.Composition;
using Asv.Cfg;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Core;

[Export(HeaderMenuItem.UriString + "/tools", typeof(IHeaderMenuItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class HeaderTemplaterMenu : HeaderMenuItem
{
    public const string UriString = HeaderMenuItem.UriString + "tools/templater";
    public static readonly Uri Uri = new(UriString);
    private readonly IConfiguration _cfg;

    [ImportingConstructor]
    public HeaderTemplaterMenu(IConfiguration cfg):base(Uri)
    {
        _cfg = cfg;
        Header = RS.HeaderTemplaterMenu_Header;
        Icon = MaterialIconKind.CalendarBlank;
        Order = 1;
        HotKey = KeyGesture.Parse("Alt+T");
        Command = ReactiveCommand.CreateFromTask(ExecuteImpl);
    }
    
    private async Task ExecuteImpl(CancellationToken cancel)
    {
        var dialog = new ContentDialog
        {
            Title = RS.HeaderTemplaterMenu_Header,
            CloseButtonText = RS.TemplaterDialog_CloseButton,
            PrimaryButtonText = RS.TemplaterDialog_SaveButton
        };
        using var viewModel = new TemplaterViewModel(_cfg);
        viewModel.ApplyDialog(dialog);
        dialog.Content = viewModel;
        var result = await dialog.ShowAsync();
    }
}