using Microsoft.UI.Xaml;

namespace Desktiny.UI.Tools
{
    public static class WinUI3Helper
    {
        public static DependencyObject FindChildElementByName(DependencyObject tree, string sName)
        {
            for (int i = 0; i < Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(tree); i++)
            {
                DependencyObject child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(tree, i);
                if (child != null && ((FrameworkElement)child).Name == sName)
                    return child;
                else
                {
                    DependencyObject childInSubtree = FindChildElementByName(child, sName);
                    if (childInSubtree != null)
                        return childInSubtree;
                }
            }
            return null;
        }

    }
}
