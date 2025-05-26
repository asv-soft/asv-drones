using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones;

[ExportViewFor(typeof(FileBrowserViewModel))]
public partial class FileBrowserView : UserControl
{
    public FileBrowserView()
    {
        InitializeComponent();
    }
}
