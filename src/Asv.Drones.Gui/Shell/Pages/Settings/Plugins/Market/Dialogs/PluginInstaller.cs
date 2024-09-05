using System.Threading.Tasks;
using Asv.Cfg;
using Asv.Drones.Gui.Api;
using FluentAvalonia.UI.Controls;

namespace Asv.Drones.Gui;

public class PluginInstaller(
    IConfiguration cfg,
    ILogService log,
    IPluginManager manager
)
{
    public async Task ShowInstallDialog()
    {
        var dialog = new ContentDialog
        {
            Title = RS.PluginInstallerViewModel_InstallDialog_Title,
            CloseButtonText = RS.PluginInstallerViewModel_InstallDialog_SecondaryButtonText,
            PrimaryButtonText = RS.PluginInstallerViewModel_InstallDialog_PrimaryButtonText,
        };

        using var viewModel = new PluginInstallerViewModel(
            cfg,
            log,
            manager
        );

        viewModel.ApplyDialog(dialog);

        dialog.Content = viewModel;
        await dialog.ShowAsync();
    }
}