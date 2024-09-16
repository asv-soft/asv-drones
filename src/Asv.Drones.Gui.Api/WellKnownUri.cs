namespace Asv.Drones.Gui.Api;

public static class WellKnownUri
{
    /// <summary>
    /// This is Scheme for URI in this application
    /// </summary>
    public const string UriScheme = "asv";

    /// <summary>
    /// This simple non empty URI
    /// </summary>
    public const string Undefined = $"{UriScheme}:null";

    public static readonly Uri UndefinedUri = new(Undefined);

    /// <summary>
    /// This is base URI for all shell controls
    /// </summary>
    public const string Shell = $"{UriScheme}:shell";

    /// <summary>
    /// This base URI for left menu tms with shell pages  SHELL=>MENU controls
    /// </summary>
    public const string ShellMenu = $"{Shell}.menu";

    /// <summary>
    /// This base URI for SHELL=>HEADER controls
    /// </summary>
    public const string ShellHeader = $"{Shell}.header";

    /// <summary>
    /// This is base uri for SHELL=>HEADER=>MENU
    /// </summary>
    public const string ShellHeaderMenu = $"{ShellHeader}.menu";

    /// <summary>
    /// This is base URI for SHELL=>STATUS controls
    /// </summary>
    public const string ShellStatus = $"{Shell}.status";

    /// <summary>
    /// This is base URI for SHELL=>PAGES
    /// </summary>
    public const string ShellPage = $"{Shell}.page";


    public const string ShellStatusTextMessage = $"{ShellStatus}.text-message";
    public const string ShellStatusMapCache = $"{ShellStatus}.map-cache";
    public const string ShellStatusPorts = $"{ShellStatus}.ports";


    public const string ShellMenuMapFlight = $"{ShellMenu}.flight";
    public static readonly Uri ShellMenuMapFlightUri = new(ShellMenuMapFlight);
    public const string ShellPageMapFlight = $"{ShellPage}.flight";
    public static readonly Uri ShellPageMapFlightUri = new(ShellPageMapFlight);
    public const string ShellPageMapFlightAction = $"{ShellPageMapFlight}.action";
    public const string ShellPageMapFlightActionZoom = $"{ShellPageMapFlightAction}.zoom";
    public const string ShellPageMapFlightActionRuler = $"{ShellPageMapFlightAction}.ruler";
    public const string ShellPageMapFlightActionMover = $"{ShellPageMapFlightAction}.mover";
    public const string ShellPageMapFlightWidget = $"{ShellPageMapFlight}.widget";
    public const string ShellPageMapFlightWidgetAnchorEditor = $"{ShellPageMapFlightWidget}.editor";
    public const string ShellPageMapFlightWidgetUav = $"{ShellPageMapFlightWidget}.uav";
    public const string ShellPageMapFlightWidgetLogger = $"{ShellPageMapFlightWidget}.logger";
    public const string ShellPageMapFlightAnchor = $"{ShellPageMapFlight}.layer";


    public const string ShellMenuMapPlaning = $"{ShellMenu}.planing";
    public static readonly Uri ShellMenuMapPlaningUri = new(ShellMenuMapPlaning);
    public const string ShellPageMapPlaning = $"{ShellPage}.planing";
    public static readonly Uri ShellPageMapPlaningUri = new(ShellPageMapPlaning);
    public const string ShellPageMapPlaningAction = $"{ShellPageMapPlaning}.action";
    public const string ShellPageMapPlaningActionZoom = $"{ShellPageMapPlaningAction}.zoom";
    public const string ShellPageMapPlaningActionRuler = $"{ShellPageMapPlaningAction}.ruler";
    public const string ShellPageMapPlaningActionMover = $"{ShellPageMapPlaningAction}.mover";
    public const string ShellPageMapPlaningMissionBrowser = $"{ShellPageMapPlaning}.browser";
    public const string ShellPageMapPlaningMissionSavingBrowser = $"{ShellPageMapPlaning}.browser";
    public static readonly Uri ShellPageMapPlaningMissionBrowserUri = new(ShellPageMapPlaningMissionBrowser);
    public static readonly Uri ShellPageMapPlaningMissionSavingBrowserUri = new(ShellPageMapPlaningMissionSavingBrowser);
    public const string ShellPageMapPlaningWidget = $"{ShellPageMapPlaning}.widget";
    public const string ShellPageMapPlaningWidgetAnchorEditor = $"{ShellPageMapPlaningWidget}.editor";
    public const string ShellPageMapPlaningWidgetEditor = $"{ShellPageMapPlaningWidget}.mission-editor";
    public const string ShellPageMapPlaningWidgetItemEditor = $"{ShellPageMapPlaningWidget}.item-editor";

    public const string ShellPageMapPlaningWidgetEditorUploadMissionDialog =
        $"{ShellPageMapPlaningWidgetEditor}.upload-mission-dialog";

    public const string ShellPageMapPlaningWidgetEditorDownloadMissionDialog =
        $"{ShellPageMapPlaningWidgetEditor}.download-mission-dialog";

    public const string ShellPagePacketViewer = $"{ShellPage}.packets";
    public static readonly Uri ShellPagePacketViewerUri = new(ShellPagePacketViewer);
    public const string ShellMenuPacketViewer = $"{ShellMenu}.packets";


    public const string ShellMenuSettings = $"{ShellMenu}.settings";
    public static readonly Uri ShellMenuSettingsUri = new(ShellPageSettings);
    public const string ShellPageSettings = $"{ShellPage}.settings";
    public static readonly Uri ShellPageSettingsUri = new(ShellPageSettings);

    public const string ShellPageSettingsConnections = $"{ShellPageSettings}.connection";
    public static readonly Uri ShellPageSettingsConnectionsUri = new(ShellPageSettingsConnections);

    public const string ShellPageSettingsConnectionsIdentify = $"{ShellPageSettingsConnections}.id";
    public static readonly Uri ShellPageSettingsConnectionsIdentifyUri = new(ShellPageSettingsConnectionsIdentify);

    public const string ShellPageSettingsConnectionsDevices = $"{ShellPageSettingsConnections}.devices";
    public static readonly Uri ShellPageSettingsConnectionsDevicesUri = new(ShellPageSettingsConnectionsDevices);

    public const string ShellPageSettingsConnectionsPorts = $"{ShellPageSettingsConnections}.ports";
    public static readonly Uri ShellPageSettingsConnectionsPortsUri = new(ShellPageSettingsConnectionsPorts);

    public const string ShellPageSettingsAppearance = $"{ShellPageSettings}.appearance";
    public static readonly Uri ShellPageSettingsAppearanceUri = new Uri(ShellPageSettingsAppearance);

    public const string ShellPageSettingsMeasure = $"{ShellPageSettings}.measure";
    public static readonly Uri ShellPageSettingsMeasureUri = new(ShellPageSettingsMeasure);

    public const string ShellPageSettingsPlugins = $"{ShellPageSettings}.plugins";
    public static readonly Uri ShellPageSettingsPluginsUri = new(ShellPageSettingsPlugins);

    public const string ShellPageSettingsPluginsMarket = $"{ShellPageSettingsPlugins}.market";
    public static readonly Uri ShellPageSettingsPluginsMarketUri = new(ShellPageSettingsPluginsMarket);

    public const string ShellPageSettingsPluginsLocal = $"{ShellPageSettingsPlugins}.local";
    public static readonly Uri ShellPageSettingsPluginsLocalUri = new(ShellPageSettingsPluginsLocal);

    public const string ShellPageSettingsPluginsSource = $"{ShellPageSettingsPlugins}.source";
    public static readonly Uri ShellPageSettingsPluginsSourceUri = new(ShellPageSettingsPluginsSource);

    public const string ShellMenuParamsVehicle = $"{ShellMenu}.params-vehicle";
    public const string ShellPageParamsVehicle = $"{ShellPage}.params-vehicle";

    public const string ShellMenuQuickParamsVehicle = $"{ShellMenu}.quick-params-vehicle";
    
    public const string ShellPageQuickParams = $"{ShellPage}.quick-params";
    
    public const string ShellPageQuickParamsArduCopterVehicle = $"{ShellPageQuickParams}.ardu-copter";

    public const string ShellPageQuickParamsArduPlaneVehicle = $"{ShellPageQuickParams}.ardu-plane";

    public const string ShellPageQuickParamsPx4CopterVehicle = $"{ShellPageQuickParams}.px-4-copter";

    public const string ShellPageQuickParamsPx4PlaneVehicle = $"{ShellPageQuickParams}.px-4-plane";
    
    public const string ShellPageQuickParamsArduCopterVehicleStandard = $"{ShellPageQuickParamsArduCopterVehicle}.standard-params";
    
    public const string ShellPageQuickParamsArduPlaneVehicleStandard = $"{ShellPageQuickParamsArduPlaneVehicle}.standard-params";
    
    public const string ShellPageQuickParamsPx4CopterVehicleStandard = $"{ShellPageQuickParamsPx4CopterVehicle}.standard-params";
    
    public const string ShellPageQuickParamsPx4PlaneVehicleStandard = $"{ShellPageQuickParamsPx4PlaneVehicle}.standard-params";
    
    public const string ShellMenuLogViewer = $"{ShellMenu}.log-viewer";
    public static readonly Uri ShellMenuLogViewerUri = new(ShellMenuLogViewer);
    public const string ShellPageLogViewer = $"{ShellPage}.log-viewer";
    public static readonly Uri ShellPageLogViewerUri = new(ShellPageLogViewer);
}