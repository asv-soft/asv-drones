using System.ComponentModel.Composition;
using System.Globalization;
using Asv.Cfg;
using Asv.Common;

namespace Asv.Drones.Gui.Core
{
    public class LocalizationServiceConfig
    {
        public string? CurrentLanguage { get; set; }
    }

    public class LocalizationServiceBase : ServiceWithConfigBase<LocalizationServiceConfig>, ILocalizationService
    {
        private readonly List<LanguageInfo> _languages = new()
        {
            new LanguageInfo("en","English (EN)", ()=>CultureInfo.GetCultureInfo("en") ),
            new LanguageInfo("ru","Русский (RU)", ()=>CultureInfo.GetCultureInfo("ru") ),
        };

        [ImportingConstructor]
        public LocalizationServiceBase(IConfiguration cfgSvc):base(cfgSvc)
        {
            var selectedLang = default(LanguageInfo);
            var langFromConfig = InternalGetConfig(_ => _.CurrentLanguage);
            if (string.IsNullOrWhiteSpace(langFromConfig) == false)
            {
                selectedLang = _languages.FirstOrDefault(_ => _.Id.Equals(langFromConfig));
            }

            selectedLang ??= _languages.First();
            CurrentLanguage = new RxValue<LanguageInfo>(selectedLang).DisposeItWith(Disposable);
            CurrentLanguage.Subscribe(SetLanguage).DisposeItWith(Disposable);
        }

        public IRxEditableValue<LanguageInfo> CurrentLanguage { get; }
        public IEnumerable<LanguageInfo> AvailableLanguages => _languages;

        private void SetLanguage(LanguageInfo lang)
        {
            if (lang == null) throw new ArgumentNullException(nameof(lang));
            var culture = lang.Culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            InternalSaveConfig(_ => _.CurrentLanguage = lang.Id);

        }

        #region Units

        public const long OneKB = 1024;

        public const long OneMB = OneKB * OneKB;

        public const long OneGB = OneMB * OneKB;

        public const long OneTB = OneGB * OneKB;

        public string BytesToString(long bytes)
        {
            //TODO: Localize
            return bytes switch
            {
                (< OneKB) => $"{bytes}B",
                (>= OneKB) and (< OneMB) => $"{bytes / OneKB}KB",
                (>= OneMB) and (< OneGB) => $"{bytes / OneMB}MB",
                (>= OneGB) and (< OneTB) => $"{bytes / OneMB}GB",
                (>= OneTB) => $"{bytes / OneTB}"
            };
        }

        public string BytesRateToString(long bytesPerSec)
        {
            //TODO: Localize
            return bytesPerSec switch
            {
                (< OneKB) => $"{bytesPerSec}B/s",
                (>= OneKB) and (< OneMB) => $"{bytesPerSec / OneKB}KB/s",
                (>= OneMB) and (< OneGB) => $"{bytesPerSec / OneMB}MB/s",
                (>= OneGB) and (< OneTB) => $"{bytesPerSec / OneMB}GB/s",
                (>= OneTB) => $"{bytesPerSec / OneTB}"
            };
        }

        #endregion  


    }
}