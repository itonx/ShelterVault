using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PageTypeAttribute : Attribute
    {
        public Type PageType { get; }

        public PageTypeAttribute(Type pageType)
        {
            PageType = pageType;
        }
    }
}
