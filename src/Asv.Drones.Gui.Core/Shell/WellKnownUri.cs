namespace Asv.Drones.Gui.Core
{
    public static class WellKnownUri
    {
        public const string ShellBaseUriString = "asv:shell";
        public static readonly Uri ShellBaseUri = new(ShellBaseUriString);

        public const string ShellMenuUriString = $"{ShellBaseUriString}.menu";
        public static readonly Uri ShellMenuUri = new(ShellMenuUriString);

        public const string ShellPageUriString = $"{ShellBaseUriString}.page";
        public static readonly Uri ShellPageUri = new(ShellPageUriString);

        public const string ShellStatusUriString = $"{ShellBaseUriString}.status";
        public static readonly Uri ShellStatusUri = new(ShellStatusUriString);

        public const string ShellMenuSettingsUriString = $"{ShellMenuUriString}.settings";
        public static readonly Uri ShellMenuSettingsUri = new(ShellMenuSettingsUriString);

        public const string ShellMenuPlaningUriString = $"{ShellMenuUriString}.planing";
        public static readonly Uri ShellMenuPlaningUri = new(ShellMenuPlaningUriString);

       

        public const string ShellPageSettingsUriString = $"{ShellPageUriString}.settings";
        public static readonly Uri ShellPageSettingsUri = new(ShellPageSettingsUriString);

        public const string ShellPageSettingsThemeUriString = $"{ShellPageSettingsUriString}.theme";
        public static readonly Uri ShellPageSettingsThemeUri = new(ShellPageSettingsThemeUriString);

        public const string ShellPagePlaningUriString = $"{ShellPageUriString}.planing";
        public static readonly Uri ShellPagePlaningUri = new(ShellPagePlaningUriString);

       
    }
}