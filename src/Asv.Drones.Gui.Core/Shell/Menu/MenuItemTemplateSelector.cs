using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
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

                    [!NavigationViewItem.IconSourceProperty] = new Binding(nameof(IShellMenuItem.Icon)),
                    [!ContentControl.ContentProperty] = new Binding(nameof(IShellMenuItem.Name)),
                    [!NavigationViewItem.MenuItemsSourceProperty] = new Binding(nameof(IShellMenuItem.Items)),
                    [!ListBoxItem.IsSelectedProperty] = new Binding(nameof(IShellMenuItem.IsSelected)),
                    [!Visual.IsVisibleProperty] = new Binding(nameof(IShellMenuItem.IsVisible), BindingMode.TwoWay),
                    SelectsOnInvoked = true,
                    /*[!NavigationViewItem.SelectsOnInvokedProperty] =  new Binding(nameof(IShellMenuItem.NavigateTo))
                    {
                        Converter = ObjectConverters.IsNotNull
                    }*/

                });
            HeaderTemplate = new FuncDataTemplate<IShellMenuItem>((value, namescope) =>
                new NavigationViewItemHeader()
                {
                    [!ContentControl.ContentProperty] = new Binding(nameof(IShellMenuItem.Name)),
                });
        }

        public static readonly MenuItemTemplateSelector Instance = new();
        

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

        protected override IDataTemplate SelectTemplateCore(object item, Control container)
        {
            return SelectTemplateCore(item);
        }
    }
}