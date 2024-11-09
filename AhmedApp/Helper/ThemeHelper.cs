using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics; 

#if ANDROID
using Android.Content.Res;
#elif IOS
using UIKit;
#elif WINDOWS
using Microsoft.Maui.Controls;
using Windows.UI.ViewManagement;
using Windows.UI;
#endif

namespace AhmedApp.Helper
{
    public static class ThemeHelper
    {
        public static bool IsDarkMode()
        {
#if ANDROID
            var uiModeFlags = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.Resources.Configuration.UiMode & UiMode.NightMask;
            return uiModeFlags == UiMode.NightYes;
#elif IOS
        return UITraitCollection.CurrentTraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark;
#elif WINDOWS
        var uiSettings = new UISettings();
        var foregroundColor = uiSettings.GetColorValue(UIColorType.Background);

        // تحويل اللون إلى النوع الصحيح والتحقق من اللون
        var isDarkMode = (foregroundColor.R == 0 && foregroundColor.G == 0 && foregroundColor.B == 0);
        return isDarkMode;
#else
        return false;
#endif
        }

        public static void KeepScreenOn()
        {
            DeviceDisplay.KeepScreenOn = true;
        }

        // لإعادة الشاشة إلى الوضع الطبيعي
        public static void AllowScreenOff()
        {
            DeviceDisplay.KeepScreenOn = false;
        }
    }
}