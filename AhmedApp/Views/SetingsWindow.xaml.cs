using AhmedApp.Services;

#if ANDROID
using Android.OS;
using Android;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
#endif

namespace AhmedApp.Views;

public partial class SetingsWindow : ContentView
{
    BasePage basePage;
    public SetingsWindow()
    {
        InitializeComponent();
        RequestPermissions();

        basePage = Application.Current.MainPage as BasePage;
    }

    async void RequestPermissions()
    {
#if ANDROID
        if(Build.VERSION.SdkInt >= BuildVersionCodes.R) // Android 11 وما فوق
        {
            if(ContextCompat.CheckSelfPermission(Android.App.Application.Context , Manifest.Permission.ManageExternalStorage) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(Platform.CurrentActivity , new string[]
                {
                Manifest.Permission.ManageExternalStorage
                } , 1);
            }
            else
            {
                Console.WriteLine("إذن إدارة التخزين ممنوح بالفعل.");
            }
        }
        else if(Build.VERSION.SdkInt == BuildVersionCodes.Q) // Android 10
        {
            if(ContextCompat.CheckSelfPermission(Android.App.Application.Context , Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted ||
                ContextCompat.CheckSelfPermission(Android.App.Application.Context , Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(Platform.CurrentActivity , new string[]
                {
                Manifest.Permission.ReadExternalStorage,
                Manifest.Permission.WriteExternalStorage
                } , 1);
            }
            else
            {
                Console.WriteLine("أذونات القراءة والكتابة على التخزين الخارجي ممنوحة بالفعل.");
            }
        }
        else // الإصدارات الأقدم من Android 10
        {
            if(ContextCompat.CheckSelfPermission(Android.App.Application.Context , Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted ||
                ContextCompat.CheckSelfPermission(Android.App.Application.Context , Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(Platform.CurrentActivity , new string[]
                {
                Manifest.Permission.ReadExternalStorage,
                Manifest.Permission.WriteExternalStorage
                } , 1);
            }
            else
            {
                Console.WriteLine("أذونات القراءة والكتابة على التخزين الخارجي ممنوحة بالفعل.");
            }
        }
#endif
    }

    private void LoadingIndicators(bool Isloding)
    {
        LoadingIndicator.IsRunning = Isloding;
        LoadingIndicator.IsVisible = Isloding;
    }

    private async void UpdateButton_Clicked(object sender , EventArgs e)
    {
        try
        {
            // تشغيل مؤشر التحميل
            LoadingIndicators(true);

            DatabaseServices databaseServices = new(this);
            await databaseServices.LoadDataFromExcelAsync();
        }
        catch(Exception ex)
        {
            basePage.DisplayAlert("خطأ" , ex.Message , "موافق");
        }
        finally
        {
            //   إيقاف مؤشر التحميل
            LoadingIndicators(false);
        }
    }
    private async void ClearTableButton_Clicked(object sender , EventArgs e)
    {
        bool answer = await basePage.DisplayAlert("تأكيد الحذف" , "سيتم حذف بيانات الجدول؟" , "نعم" , "لا");
        if(answer)
        {
            try
            {
                // تشغيل مؤشر التحميل
                LoadingIndicators(true);

                DatabaseServices databaseServices = new(this);
                await databaseServices.ClearTableDataAsync();
            }
            catch(Exception)
            {
                await MessageService.ShowMessage("معلومة" , "لا يوجد بيانات لعرضها." , Colors.Yellow);
            }
            finally
            {
                //    إيقاف مؤشر التحميل
                LoadingIndicators(false);
            }
        }
    }

    private async void DropTableButton_Clicked(object sender , EventArgs e)
    {

        bool answer = await basePage.DisplayAlert("تأكيد الحذف" , "هل تريد حذف الجدول؟" , "نعم" , "لا");
        if(answer)
        {
            try
            {
                // تشغيل مؤشر التحميل
                LoadingIndicators(true);

                DatabaseServices databaseServices = new(this);
                await databaseServices.DropTableAsync();
            }
            catch(Exception)
            {
                await MessageService.ShowMessage("معلومة" , "لا يوجد جدول لحذفه." , Colors.Yellow);
            }
            finally
            {
                //    إيقاف مؤشر التحميل
                LoadingIndicators(false);
            }
        }
    }

    private async void DeleteDatabaseButton_Clicked(object sender , EventArgs e)
    {

        bool answer = await basePage.DisplayAlert("تأكيد الحذف" , "هل تريد حذف قاعدة البيانات؟" , "نعم" , "لا");
        if(answer)
        {
            try
            {
                // تشغيل مؤشر التحميل
                LoadingIndicators(true);

                DatabaseServices databaseServices = new(this);
                databaseServices.DeleteDatabase();
            }
            catch(Exception)
            {
                await MessageService.ShowMessage("معلومة" , "لا يوجد قاعدة بيانات لحذفها." , Colors.Yellow);
            }
            finally
            {
                //    إيقاف مؤشر التحميل
                LoadingIndicators(false);
            }
        }
    }

    private async void DeleteExcelFileButton_Clicked(object sender , EventArgs e)
    {

        bool answer = await basePage.DisplayAlert("تأكيد الحذف" , "هل تريد حذف ملف Excel؟" , "نعم" , "لا");
        if(answer)
        {
            try
            {
                // تشغيل مؤشر التحميل
                LoadingIndicators(true);

                DatabaseServices databaseServices = new(this);
                databaseServices.DeleteExcelFile();
            }
            catch(Exception)
            {
                await MessageService.ShowMessage("معلومة" , "لا يوجد ملف Excel لحذفها." , Colors.Yellow);
            }
            finally
            {
                //    إيقاف مؤشر التحميل
                LoadingIndicators(false);
            }
        }
    }

    private async void OnExportButtonClicked(object sender , EventArgs e)
    {
        DatabaseServices databaseServices = new(this);
        databaseServices.ExportDatabase();
        await MessageService.ShowMessage("معلومة" , "تم التصدير بنجاح." , Colors.Yellow);
    }

    private async void OnImportButtonClicked(object sender , EventArgs e)
    {
        DatabaseServices databaseServices = new(this);
        await databaseServices.ImportDatabaseFileAsync();
        await MessageService.ShowMessage("معلومة" , "تم الاستيراد بنجاح." , Colors.Yellow);
    }

    private async void OnShareButtonClicked(object sender , EventArgs e)
    {
        DatabaseServices databaseServices = new(this);
        await databaseServices.ShareDatabaseAsync();
        await MessageService.ShowMessage("معلومة" , "تم التصدير بنجاح." , Colors.Yellow);
    }
}