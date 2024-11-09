using AhmedApp.Models;
using AhmedApp.Services;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.Diagnostics;

#if ANDROID
using Android;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
#endif

namespace AhmedApp.Views;

public partial class SpaerPartsPage : ContentView
{
    private BasePage basePage;
    private static string _dbPath;
    private int _rowsPerPage = 15;
    private int _currentPageIndex = 0;
    private ObservableCollection<Models.SpaerPart> _currentPageData;
    private DatabaseServices _databaseServices;

    public SpaerPartsPage()
    {
        InitializeComponent();
        _databaseServices = new DatabaseServices(this); // تمرير الصفحة الحالية
        _dbPath = _databaseServices.GetDatabasePath();
        _currentPageData = [];

        RequestPermissions();
        basePage = Application.Current.MainPage as BasePage;
    }

    async void RequestPermissions()
    {
#if ANDROID
        if(Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.R) // Android 11 وما فوق
        {
            if(ContextCompat.CheckSelfPermission(Android.App.Application.Context , Manifest.Permission.ManageExternalStorage) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(Platform.CurrentActivity , new string[]
                {
                        Manifest.Permission.ManageExternalStorage
                } , 1);
            }
        }
        else if(Android.OS.Build.VERSION.SdkInt == Android.OS.BuildVersionCodes.Q) // Android 10
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
        }
#endif
    }

    public string ConvertArabicNumbersToEnglish(string input)
    {
        string[] arabicDigits = { "٠" , "١" , "٢" , "٣" , "٤" , "٥" , "٦" , "٧" , "٨" , "٩" };

        for(int i = 0; i < arabicDigits.Length; i++)
        {
            input = input.Replace(arabicDigits[i] , i.ToString());
        }

        return input;
    }

    public static string ConvertArabicNumbersInText(string input)
    {
        var arabicToEnglishDigits = new Dictionary<char , char>
        {
            {'٠', '0'}, {'١', '1'}, {'٢', '2'}, {'٣', '3'},
            {'٤', '4'}, {'٥', '5'}, {'٦', '6'}, {'٧', '7'},
            {'٨', '8'}, {'٩', '9'}
        };

        return new string(input.Select(ch => arabicToEnglishDigits.ContainsKey(ch) ? arabicToEnglishDigits[ch] : ch).ToArray());
    }

    private void LoadingIndicators(bool Isloding)
    {
        LoadingIndicator.IsRunning = Isloding;
        LoadingIndicator.IsVisible = Isloding;
    }

    //[Obsolete]
    // تحسين دالة تحميل البيانات
    private async void LoadPageData(int pageIndex)
    {
        try
        {
            // عرض مؤشر التحميل للمستخدم أثناء جلب البيانات
            LoadingIndicators(true);

            // تنفيذ العملية في خلفية للتأكد من أن واجهة المستخدم لا تتجمد
            await Task.Run(async () =>
     {
         // حساب الأوفست بناءً على الصفحة الحالية وعدد الصفوف لكل صفحة
         int offset = (pageIndex - 1) * _rowsPerPage;
         string query = $"SELECT * FROM SpaerPart LIMIT {_rowsPerPage} OFFSET {offset}";

         Debug.WriteLine("LoadPageData query: " + query);
         ObservableCollection<Models.SpaerPart> results = [];

         using(var connection = new SqliteConnection($"Data Source={_dbPath}"))
         {
             connection.Open();
             using var command = new SqliteCommand(query , connection);
             using var reader = command.ExecuteReader();
             while(reader.Read())
             {
                 results.Add(new Models.SpaerPart
                 {
                     SapCode = Convert.ToInt32(reader["SapCode"]) ,
                     PartNo = reader["partNo"].ToString() ,
                     MatrialGroup = reader["MatrialGroup"].ToString() ,
                     Model = reader["Model"].ToString() ,
                     DescriptionAR = reader["DescriptionAR"].ToString() ,
                     DescriptionEN = reader["DescriptionEN"].ToString() ,
                     C1 = reader["C1"].ToString() ,
                     C2 = reader["C2"].ToString() ,
                     IsDamaged = Convert.ToBoolean(reader["IsDamaged"]) ,
                     ANots = reader["ANots"].ToString()
                 });
             }
         }

         // تحديث الواجهة الرئيسية
         Device.BeginInvokeOnMainThread(async () =>
         {
             _currentPageData = results;

             NoDataLabel.IsVisible = results.Count <= 0;
             NoDataLabel.Text = results.Count <= 0 ? "لا يوجد بيانات" : string.Empty;
         });
     });
            DisplayResults(_currentPageData , pageIndex);
        }
        catch(Exception ex)
        {
            await MessageService.ShowMessage("خطأ: " , $"Error getting total row count: {ex.Message}" , Colors.LawnGreen , true);
        }
        finally
        {
            LoadingIndicators(false);
        }
    }

    // [Obsolete]
    private void LoadButton_Clicked(object sender , EventArgs e)
    {
        PerformSearch();
    }

    //  [Obsolete]
    private void OnEntryCompleted(object sender , EventArgs e)
    {
        PerformSearch();
    }

    // [Obsolete]
    private void OnSearchButtonClicked(object sender , EventArgs e)
    {
        PerformSearch();
    }

    private async void PerformSearch()
    {
        LoadingIndicators(true);
        try
        { // تحويل الأرقام العربية إلى أرقام إنجليزية للتعامل معها في قاعدة البيانات
            // تجهيز معايير البحث من المدخلات
            string descriptionAR = ConvertArabicNumbersInText(SearchNameEntry.Text?.Trim() ?? string.Empty);
            string model = ConvertArabicNumbersInText(SearchModelEntry.Text?.Trim() ?? string.Empty);
            string matrialGroup = ConvertArabicNumbersInText(SearchGropEntry.Text?.Trim() ?? string.Empty);
            string sapCode = ConvertArabicNumbersInText(SearchCodeEntry.Text?.Trim() ?? string.Empty);
            string partNo = ConvertArabicNumbersInText(SearchPartNoEntry.Text?.Trim() ?? string.Empty);
            string c1 = ConvertArabicNumbersInText(SearchC1Entry.Text?.Trim() ?? string.Empty);

            await Task.Run(async () =>
            {
                _currentPageData = await _databaseServices.PerformSearchAsync(descriptionAR , model , matrialGroup , sapCode , partNo , c1).ConfigureAwait(false);
             
                Device.InvokeOnMainThreadAsync(async () =>
               {
                   DisplayResults(_currentPageData , _currentPageIndex);
               });
            });
        }
        catch(Exception ex)
        {
            await MessageService.ShowMessage("خطأ: " , ex.Message , Colors.IndianRed , true);
        }
        finally
        {
            LoadingIndicators(false);
        }
    }

    // هذه الدالة لعرض النتائج بناءً على البيانات المحملة
    private void DisplayResults(ObservableCollection< SpaerPart> results , int pageIndex)
    {
        try
        {
            // حساب الأوفست بناءً على الصفحة الحالية وعدد الصفوف لكل صفحة
            int offset = (pageIndex) * _rowsPerPage;
            var data = results.Skip(offset).Take(_rowsPerPage).ToList();

            DataCollectionView.ItemsSource = data;

            // إظهار أو إخفاء رسالة "لا يوجد بيانات" بناءً على النتيجة
            if(results.Count == 0)
            {
                NoDataLabel.IsVisible = results.Count == 0;
                NoDataLabel.Text = results.Count == 0 ? "لا يوجد بيانات" : string.Empty;
                _ = MessageService.ShowMessage("خطأ: " , "لا يوجد بيانات" , Colors.LawnGreen , true);
            }
            // تحديث واجهة التنقل
            UpdatePaginationUI(pageIndex);
        }
        catch(Exception ex)
        {
            _ = MessageService.ShowMessage("خطأ: " , "حدث خطا اثناء تحميل البيانات: " + ex.Message , Colors.IndianRed , true);
        }
        finally
        {
            LoadingIndicators(false);
        }
    }

    // هذه الدالة لتحديث واجهة المستخدم المتعلقة بالتنقل بين الصفحات
    private async void UpdatePaginationUI(int pageIndex)
    {
        try
        {
            int totalRows = _currentPageData.Count();
            // حساب عدد الصفحات بناءً على إجمالي البيانات
            int totalPages = GetTotalPages();

            // تحديث الأزرار (السابق/التالي) بناءً على الصفحة الحالية
            PreviousButton.IsEnabled = pageIndex >= 1;
            NextButton.IsEnabled = pageIndex + 1 < totalPages;

            _currentPageIndex = pageIndex;
            PagesNumber.Text = $"{_currentPageIndex + 1}/{totalPages}";

            RowNumber.Text = $"عدد الصفوف ( {totalRows} )";

            await MessageService.ShowMessage("" , $"Page {pageIndex} of {_currentPageIndex + 1}" , Colors.LightGreen);
        }
        catch(Exception ex)
        {
            await MessageService.ShowMessage("خطأ: " , ex.Message , Colors.IndianRed , true);
        }
    }

    // هذه الدالة لتحصل على العدد الإجمالي للصفوف في جدول SpaerPart
    public static async Task<int> GetTotalRowCount()
    {
        int count = 0;
        try
        {
            string query = "SELECT COUNT(*) FROM SpaerPart";

            using(var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                connection.Open();
                using var command = new SqliteCommand(query , connection);
                count = Convert.ToInt32(command.ExecuteScalar());
            }

            if(count <= 0)
            {
                await MessageService.ShowMessage("معلومة: " , "لا يوجد بيانات" , Colors.LawnGreen);
            }
        }
        catch(Exception ex)
        {
            // معالجة الأخطاء 
            await MessageService.ShowMessage("خطأ: " , $"Error getting total row count: {ex.Message}" , Colors.LawnGreen , true);
            return 0;
        }
        return count;
    }

    // هذه الدالة لحدث التنقل بين الصفحات
    private void NextPage_Clicked(object sender , EventArgs e)
    {
        int nextPage = 0;
        // نتحقق من الصفحة الحالية وننتقل إلى الصفحة التالية إذا لم نكن في آخر صفحة
        if(_currentPageIndex <= 0)
        {
            _currentPageIndex = 1;
        }
        else
        {
            _currentPageIndex++;
        }
        nextPage = _currentPageIndex;
        DisplayResults(_currentPageData , nextPage);
    }

    private void PreviousPage_Clicked(object sender , EventArgs e)
    {
        // نتحقق من الصفحة الحالية وننتقل إلى الصفحة السابقة إذا لم نكن في أول صفحة
        int previousPage = _currentPageIndex - 1;
        DisplayResults(_currentPageData , previousPage);
    }

    private int GetTotalPages()
    {
        return (int)Math.Ceiling((double)_currentPageData.Count / _rowsPerPage);
    }
}