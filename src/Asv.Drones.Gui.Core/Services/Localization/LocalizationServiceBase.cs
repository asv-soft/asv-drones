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

        public IMeasureUnit<double> ByteRate { get; } = new BytesRate();

        public IMeasureUnit<double> ItemsRate { get; } = new ItemsRate();

        public IMeasureUnit<long> ByteSize { get; } = new ByteSize();


        #endregion


    }
}