namespace Asv.Drones.Gui.Core
{
    public static class WellKnownUri
    {
        public const string UriScheme = "asv";
        public const string Shell = "asv:shell";

        #region Shell main menu

        public const string ShellMenu = "asv:shell.menu";
        public const string ShellMenuFlight = "asv:shell.menu.flight";
        public const string ShellMenuPlaning = "asv:shell.menu.planing";
        public const string ShellMenuSettings = "asv:shell.menu.settings";

        #endregion

        #region Shell pages

        public const string ShellPage = "asv:shell.page";
        public const string ShellPageSettings = "asv:shell.page.settings";
        public const string ShellPageSettingsTheme = "asv:shell.page.settings.theme";
        public const string ShellPageFlight = $"asv:shell.page.flight";
        public const string ShellPagePlaning = "asv:shell.page.planing";

        #endregion

        #region Status items

        public const string ShellStatus = "asv:shell.status";
        public const string ShellStatusMapCache = "asv:shell.status.mapchache";

        #endregion


    }
}