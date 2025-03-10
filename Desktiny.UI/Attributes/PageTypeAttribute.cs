using System;

namespace Desktiny.UI.Attributes
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
