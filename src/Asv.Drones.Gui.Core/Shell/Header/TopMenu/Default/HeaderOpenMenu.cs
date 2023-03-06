using System.ComponentModel.Composition;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    [Export(HeaderFileMenu.UriString, typeof(IHeaderMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class HeaderOpenMenu:HeaderMenuItem
    {
        private readonly IGlobalCommandsService _cmd;
        public const string UriString = HeaderFileMenu.UriString + "/open";
        public static readonly Uri Uri = new(UriString);

        [ImportingConstructor]
        public HeaderOpenMenu(IGlobalCommandsService cmd):base(Uri)
        {
            _cmd = cmd ?? throw new ArgumentNullException(nameof(cmd));
            Header = RS.HeaderOpenMenu_Header;
            Icon = MaterialIconKind.DatabaseOutline;
            Order = short.MinValue + 1;
            Command = cmd.OpenStore;
            HotKey = KeyGesture.Parse("Ctrl+O");
        }

        
    }
    
    [Export(HeaderFileMenu.UriString, typeof(IHeaderMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class HeaderCreateMenu:HeaderMenuItem
    {
        private readonly IGlobalCommandsService _cmd;
        public const string UriString = HeaderFileMenu.UriString + "/create";
        public static readonly Uri Uri = new(UriString);

        [ImportingConstructor]
        public HeaderCreateMenu(IGlobalCommandsService cmd):base(Uri)
        {
            _cmd = cmd ?? throw new ArgumentNullException(nameof(cmd));
            Header = "Create new";
            Icon = MaterialIconKind.DatabasePlusOutline;
            Order = short.MinValue + 2;
            Command = cmd.CreateStore;
            HotKey = KeyGesture.Parse("Ctrl+N");
        }

        
    }
   
}