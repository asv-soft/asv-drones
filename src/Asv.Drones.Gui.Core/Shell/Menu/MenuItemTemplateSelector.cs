using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using FluentAvalonia.UI.Controls;

namespace Asv.Drones.Gui.Core
{
    public class MenuItemTemplateSelector : DataTemplateSelector
    {
        private static readonly FuncDataTemplate<IShellMenuItem> SimpleItem;
        private static readonly FuncDataTemplate<IShellMenuItem> HeaderTemplate;

        static MenuItemTemplateSelector()
        {
            SimpleItem = new FuncDataTemplate<IShellMenuItem>((value, namescope) =>
                new NavigationViewItem
                {
                    [!NavigationViewItem.IconProperty] = new Binding(nameof(IShellMenuItem.Icon)),
                    [!ContentControl.ContentProperty] = new Binding(nameof(IShellMenuItem.Name)),
                    [!NavigationViewItem.MenuItemsProperty] = new Binding(nameof(IShellMenuItem.Items)),
                });
            HeaderTemplate = new FuncDataTemplate<IShellMenuItem>((value, namescope) =>
                new NavigationViewItemHeader()
                {
                    [!ContentControl.ContentProperty] = new Binding(nameof(IShellMenuItem.Name)),
                });
        }

        public static MenuItemTemplateSelector Instance = new();
        

        protected override IDataTemplate SelectTemplateCore(object item)
        {
            if (item is IShellMenuItem menuItem)
            {
                return menuItem.Type switch
                {
                    ShellMenuItemType.Header => HeaderTemplate,
                    ShellMenuItemType.Group => SimpleItem,
                    ShellMenuItemType.PageNavigation => SimpleItem,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            return null;
        }

        protected override IDataTemplate SelectTemplateCore(object item, IControl container)
        {
            return SelectTemplateCore(item);
        }
    }
}