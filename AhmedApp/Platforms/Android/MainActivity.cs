using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

namespace AhmedApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    //[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]

    public class MainActivity : MauiAppCompatActivity
    {
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        //protected override void OnCreate(Bundle savedInstanceState)
        //{
        //    base.OnCreate(savedInstanceState);
        //    Platform.Init(this, savedInstanceState);
        //}

        //protected override void OnCreate(Bundle savedInstanceState)
        //{
        //    base.OnCreate(savedInstanceState);

        //    RequestPermissions();
        //}

        //private void RequestPermissions()
        //{
        //    if(Build.VERSION.SdkInt >= BuildVersionCodes.M)
        //    {
        //        RequestPermissions(new string[] {
        //        Android.Manifest.Permission.ReadExternalStorage,
        //        Android.Manifest.Permission.WriteExternalStorage
        //    }, 0);
        //    }
        //}
        protected override void OnCreate(Bundle savedInstanceState)
        {
            if(Build.VERSION.SdkInt >= BuildVersionCodes.R) // Android 11 وما فوق
            {
                if(!Android.OS.Environment.IsExternalStorageManager)
                {
                    Intent intent = new();
                    intent.SetAction(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
                    Android.Net.Uri uri = Android.Net.Uri.FromParts("package", this.PackageName, null);
                    intent.SetData(uri);
                    StartActivity(intent);
                }
            }
            else
            {
                // طلب أذونات التخزين للإصدارات الأقدم
                RequestPermissions([Android.Manifest.Permission.ReadExternalStorage, Android.Manifest.Permission.WriteExternalStorage], 0);
            }

            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
        }
    }
}
