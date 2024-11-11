using ShelterVault.Shared.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;

namespace ShelterVault.Shared.Helpers
{
    internal static class LocalizedResourcesHelper
    {
        public static string GetString(string key)
        {
            return ResourceManager.Current.MainResourceMap.GetSubtree(LangResourceKeys.LANG_TREE).GetValue(key).ValueAsString;
        }
    }
}