using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;

namespace ShelterVault.Services
{
    internal interface ILanguageService
    {
        IReadOnlyList<string> GetShelterVaultSupportedLanguages();
        IReadOnlyList<string> GetUserMachineSupportedLanguages();
        string GetLangValue(string resourceKey);
    }

    internal class LanguageService : ILanguageService
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
            return ResourceManager.Current.MainResourceMap.GetSubtree(LangResourceKeys.LANG_TREE).GetValue(resourceKey).ValueAsString ?? string.Empty;
        }
    }

    public static class LangService
    {
        public static string GetLangValue(string resourceKey)
        {
            return ResourceManager.Current.MainResourceMap.GetSubtree(LangResourceKeys.LANG_TREE).GetValue(resourceKey).ValueAsString ?? string.Empty;
        }
    }
}
