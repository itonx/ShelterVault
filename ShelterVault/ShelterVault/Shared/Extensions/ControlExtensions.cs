using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Extensions
{
    public static class ControlExtensions
    {
        public static void AnimateOpacity(this UIElement targetControl, double fromOpacity, double toOpacity, double durationInSeconds)
        {
            Storyboard storyboard = new Storyboard();
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = fromOpacity,
                To = toOpacity,
                Duration = new Duration(TimeSpan.FromSeconds(durationInSeconds)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(opacityAnimation, targetControl);
            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin();
        }
    }
}
