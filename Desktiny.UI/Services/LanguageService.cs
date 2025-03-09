using System.Collections.Generic;
using Windows.ApplicationModel.Resources.Core;

namespace Desktiny.UI.Services
{
    public interface ILanguageService
    {
        IReadOnlyList<string> GetShelterVaultSupportedLanguages();
        IReadOnlyList<string> GetUserMachineSupportedLanguages();
        string GetLangValue(string resourceKey);
    }

    public class LanguageService : ILanguageService
    {
        public IReadOnlyList<string> GetShelterVaultSupportedLanguages()
        {

            return Windows.Globalization.ApplicationLanguages.ManifestLanguages;
        }

        public IReadOnlyList<string> GetUserMachineSupportedLanguages()
        {
            return Windows.Globalization.ApplicationLanguages.Languages;
        }

        public string GetLangValue(string resourceKey)
        {
            return ResourceManager.Current.MainResourceMap.GetSubtree(Constants.Global.LANG_TREE).GetValue(resourceKey).ValueAsString ?? string.Empty;
        }
    }

    public static class LangService
    {
        public static string GetLangValue(string resourceKey)
        {
            return ResourceManager.Current.MainResourceMap.GetSubtree(Constants.Global.LANG_TREE).GetValue(resourceKey).ValueAsString ?? string.Empty;
        }
    }
}
