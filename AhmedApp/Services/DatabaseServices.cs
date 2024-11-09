using ClosedXML.Excel;
using SQLite;
using System.Collections;
using System.Diagnostics;
using AhmedApp.Models;
using AhmedApp.Views;
using Microsoft.Data.Sqlite;

using System.Collections.ObjectModel;

#if ANDROID
using Android.Content;
using Android.Database;
using Android.Provider;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Android;
using Android.Content.PM;
using Android.OS;
#endif

namespace AhmedApp.Services;

public class DatabaseServices
{
    private static BasePage basePage;
    static string _dbPath;
    private static SQLiteAsyncConnection _connection;
    private readonly Dictionary<string , List<SpaerPart>> _cacheSpaerParts = [];
    private int _totalRows;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private const int PageSize = 20; // حجم الصفحة
    private const string DatabasefileName = "AhmedDatabase.db3";
    private const string ExcelfileName = "SparePartsDetails.xlsx";

    public DatabaseServices(ContentView page)
    {
        RequestPermissions();

        GetBasePage();
        _dbPath = GetDatabasePath();
        GetConnection();
        InitializeDatabaseAsync();
    }

    public static SQLiteAsyncConnection GetConnection()
    {
        return _connection = new SQLiteAsyncConnection(_dbPath);
    }

    private static async void RequestPermissions()
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
        }
        else if(Build.VERSION.SdkInt >= BuildVersionCodes.Q) // Android 10
        {
            if(ContextCompat.CheckSelfPermission(Android.App.Application.Context , Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(Platform.CurrentActivity , new string[]
                {
                Manifest.Permission.ReadExternalStorage
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
        RequestPermissions1(); }

   private static async void RequestPermissions1()
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

    private async Task<bool> CheckAndRequestStoragePermissionAsync()
    {
        var statusR = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        var statusW = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

        if(statusR != PermissionStatus.Granted && statusW != PermissionStatus.Granted)
        {
            statusR = await Permissions.RequestAsync<Permissions.StorageRead>();
            statusW = await Permissions.RequestAsync<Permissions.StorageWrite>();
        }
        return statusR == PermissionStatus.Granted && statusW == PermissionStatus.Granted;
    }

    public void GetBasePage()
    {
        basePage = Application.Current.MainPage as BasePage;
    }

    public string GetDatabasePath()
    { // المسار في الويندوز
      // C:\Users\Ahmed\AppData\Local\Packages\com.CodeDevelopment.Ahmedapp_gzqvqvmxtbgm6\LocalCache\Local\AhmedApp\AhmedDatabase.db3
      // C:\Users\Ahmed\AppData\Local\Packages\com.CodeDevelopment.Ahmedapp_gzqvqvmxtbgm6\LocalState\AhmedDatabase.db3

        // استخدام FileSystem.AppDataDirectory لتحديد مسار البيانات
        string AppFolder = FileSystem.AppDataDirectory;

        // تحديد مجلد قاعدة البيانات
        string dbFolder = Path.Combine(AppFolder , "AhmedApp");

        if(!Directory.Exists(dbFolder))
        {
            Directory.CreateDirectory(dbFolder);
        }

        // تحديد اسم قاعدة البيانات
        string dbPath = Path.Combine(dbFolder , DatabasefileName);

        // طباعة المسار لسهولة التتبع
        GetBasePage();

        return dbPath;
    }

    public string GetFilePath()
    {
#if ANDROID
        string downloadsFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
        string filePath = Path.Combine(downloadsFolder , "ahmedexcel.xlsx");
#else
        string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
        string filePath = Path.Combine(folderPath , ExcelfileName);
#endif
        return filePath;
    }

    public async void InitializeDatabaseAsync()
    {
        try
        {
            await _connection.CreateTableAsync<SpaerPart>();
            await MessageService.ShowMessage("معلومة" , "تم تهيئة قاعدة البيانات" , Colors.LawnGreen);
        }
        catch(Exception ex)
        {
            await MessageService.ShowMessage("خطأ أثناء تهيئة قاعدة البيانات" , ex.Message , Colors.IndianRed);
            System.Diagnostics.Debug.WriteLine("Error during database initialization: " + ex.Message);
            await basePage.DisplayAlert("خطأ" , $"حدث خطأ أثناء إنشاء الجدول: {ex.Message}" , "موافق");
        }
        // إضافة الفهارس لتحسين الأداء
        if(await CheckTableExistsAsync())
        {
            CreateIndexesAsync();
        }
    }

    private static async Task<bool> CheckTableExistsAsync()
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync().ConfigureAwait(false);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='SpaerPart';";
            var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
            return result != null;
        }
        catch
        {
            return false;
        }
    }

    private void CreateIndexesAsync()
    {
        try
        {
             _connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_sapcode ON SpaerPart (SapCode);");
             _connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_descriptionar ON SpaerPart (DescriptionAR);");
             _connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_model ON SpaerPart (Model);");
             _connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_matrialGroup ON SpaerPart (MatrialGroup);");
             _connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_partNo ON SpaerPart (PartNo);");
             _connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_descriptionen ON SpaerPart (DescriptionEN);");
             _connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_c1 ON SpaerPart (C1);");
            _= MessageService.ShowMessage("معلومة" , "تم انشاء الفهارس" , Colors.LawnGreen);
        }
        catch(Exception ex)
        {
            _= MessageService.ShowMessage("خطأ أثناء إضافة الفهارس" , ex.Message , Colors.IndianRed);
            System.Diagnostics.Debug.WriteLine("Error during database initialization: " + ex.Message);
             basePage.DisplayAlert("خطأ" , $"حدث خطأ أثناء إضافة الفهارس: {ex.Message}" , "موافق");
        }
    }

    private async Task<bool> IsDatabaseConnectedAsync()
    {
        try
        {
            using(var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                connection.Close();
                return true;
            }
        }
        catch(Exception ex)
        {
            await MessageService.ShowMessage("خطأ" , "غير قادر على الاتصال بقاعدة البيانات. تأكد من توفر الاتصال.\n"+ ex.Message+ "\n'IsDatabaseConnectedAsync'" , Colors.IndianRed);
            return false;
        }
    }

    //  [Obsolete]
    // تحسين دالة البحث
    public async Task<ObservableCollection<SpaerPart>> PerformSearchAsync(
      string descriptionAR , string model , string matrialGroup , string sapCode , string partNo , string c1)
    {
        ObservableCollection<SpaerPart> results = [];

        try
        {
            InitializeDatabaseAsync();
            // التحقق من الاتصال بقاعدة البيانات
            if(!await IsDatabaseConnectedAsync())
            {
                await MessageService.ShowMessage("خطأ" , $"لا يوجد اتصال مع قاعدة البيانات\n'PerformSearchAsync'" , Colors.LawnGreen);
                return null; // خروج من الدالة إذا لم يكن هناك اتصال
            }

            using(var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                using var command = connection.CreateCommand();

                command.CommandText = "SELECT * FROM SpaerPart WHERE 1=1";

                if(!string.IsNullOrEmpty(descriptionAR))
                {
                    command.CommandText += " AND DescriptionAR LIKE @DescriptionAR";
                    command.Parameters.AddWithValue("@DescriptionAR" , $"%{descriptionAR}%");
                }

                if(!string.IsNullOrEmpty(model))
                {
                    command.CommandText += " AND Model LIKE @Model";
                    command.Parameters.AddWithValue("@Model" , $"%{model}%");
                }

                if(!string.IsNullOrEmpty(matrialGroup))
                {
                    command.CommandText += " AND MatrialGroup LIKE @MatrialGroup";
                    command.Parameters.AddWithValue("@MatrialGroup" , $"%{matrialGroup}%");
                }

                if(!string.IsNullOrEmpty(sapCode))
                {
                    command.CommandText += " AND SapCode LIKE @SapCode";
                    command.Parameters.AddWithValue("@SapCode" , $"%{sapCode}%");
                }

                if(!string.IsNullOrEmpty(partNo))
                {
                    command.CommandText += " AND PartNo LIKE @PartNo";
                    command.Parameters.AddWithValue("@PartNo" , $"%{partNo}%");
                }

                if(!string.IsNullOrEmpty(c1))
                {
                    command.CommandText += " AND C1 LIKE @C1";
                    command.Parameters.AddWithValue("@C1" , $"%{c1}%");
                }

                using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
              
                while(await reader.ReadAsync().ConfigureAwait(false))
                {
                    var part = new SpaerPart
                    {
                        SapCode = reader.IsDBNull(reader.GetOrdinal("SapCode")) ? 0 : reader.GetInt32(reader.GetOrdinal("SapCode")) ,
                        PartNo = reader.IsDBNull(reader.GetOrdinal("PartNo")) ? string.Empty : reader.GetString(reader.GetOrdinal("PartNo")) ,
                        MatrialGroup = reader.IsDBNull(reader.GetOrdinal("MatrialGroup")) ? string.Empty : reader.GetString(reader.GetOrdinal("MatrialGroup")) ,
                        Model = reader.IsDBNull(reader.GetOrdinal("Model")) ? string.Empty : reader.GetString(reader.GetOrdinal("Model")) ,
                        DescriptionAR = reader.IsDBNull(reader.GetOrdinal("DescriptionAR")) ? string.Empty : reader.GetString(reader.GetOrdinal("DescriptionAR")) ,
                        DescriptionEN = reader.IsDBNull(reader.GetOrdinal("DescriptionEN")) ? string.Empty : reader.GetString(reader.GetOrdinal("DescriptionEN")) ,
                        C1 = reader.IsDBNull(reader.GetOrdinal("C1")) ? string.Empty : reader.GetString(reader.GetOrdinal("C1")) ,
                        C2 = reader.IsDBNull(reader.GetOrdinal("C2")) ? string.Empty : reader.GetString(reader.GetOrdinal("C2")) ,
                        IsDamaged = !reader.IsDBNull(reader.GetOrdinal("IsDamaged")) && reader.GetBoolean(reader.GetOrdinal("IsDamaged")) ,
                        ANots = reader.IsDBNull(reader.GetOrdinal("ANots")) ? string.Empty : reader.GetString(reader.GetOrdinal("ANots"))
                    };
                    
                        await MessageService.ShowMessage("معلومة" , $"part.SapCode: {part.SapCode}\n'PerformSearchAsync'" , Colors.Aquamarine);
                    
                    results.Add(part);
                }
            }

            await MessageService.ShowMessage("معلومة" , $"عدد النتائج: {results.Count}\n'PerformSearchAsync'" , Colors.LawnGreen);
        }
        catch(Exception ex)
        {
            await MessageService.ShowMessage("خطأ" , $"فشل في البحث: {ex.Message}" , Colors.Red);
        }

        return results;
    }

    private async Task<bool> CheckIfSapCodeExists(int sapCode)
    {
        var existingRows = await _connection.Table<SpaerPart>().Where(row => row.SapCode == sapCode).ToListAsync();
        return existingRows.Any();
    }

    private async Task InsertIntoTable(List<SpaerPart> rows)
    {
        try
        {
            await _connection.InsertAllAsync(rows);
        }
        catch(Exception ex)
        {
            await MessageService.ShowMessage($"خطأ أثناء إدخال البيانات الى الجدول: " , ex.Message , Colors.IndianRed);
        }
    }

    private static bool ValidateRow(Dictionary<string , object> parameters)
    {
        // مثال بسيط للتحقق من البيانات، يمكنك تعديل الشروط حسب متطلباتك
        if(parameters == null || parameters.Count == 0)
            return false;
        // التحقق من أن SapCode موجود وصحيح
        if(!parameters.ContainsKey("SapCode") || parameters["SapCode"] == null)
            return false;
        // تحقق من أن الأجزاء الأخرى ليست فارغة (مثال: PartNo)
        if(string.IsNullOrWhiteSpace(parameters["PartNo"]?.ToString()))
            return false;
        // يمكنك إضافة المزيد من التحقق حسب الحاجة
        return true;
    }

    private async Task InsertIntoTableAsync(string tableName , Dictionary<string , object> parameters)
    {
        try
        {
            using(var connection = new SQLiteConnection(_dbPath))
            {
                connection.CreateTable<SpaerPart>(); // تأكد من أن جدول SpaerPart موجود
                var sparePart = new SpaerPart
                {
                    SapCode = Convert.ToInt32(parameters["SapCode"]) ,
                    PartNo = parameters["PartNo"].ToString() ,
                    MatrialGroup = parameters["MatrialGroup"].ToString() ,
                    Model = parameters["Model"].ToString() ,
                    DescriptionAR = parameters["DescriptionAR"].ToString() ,
                    DescriptionEN = parameters["DescriptionEN"].ToString() ,
                    C1 = parameters["C1"].ToString() ,
                    C2 = parameters["C2"].ToString() ,
                    IsDamaged = Convert.ToBoolean(parameters["IsDamaged"])
                };

                connection.Insert(sparePart);
            }
        }
        catch(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"حدث خطأ أثناء إدخال البيانات: {ex.Message}");
            throw; // يمكنك معالجة الخطأ بشكل أكثر تفصيلاً إذا لزم الأمر
        }
    }

    public async Task LoadDataFromExcelAsync()
    {
        string excelFilePath = GetFilePath();
        if(!File.Exists(excelFilePath))
        {
            await MessageService.ShowMessage("خطأ" , $"الملف غير موجود: {excelFilePath}" , Colors.IndianRed);
            return;
        }

        try
        {
            using var workbook = new XLWorkbook(excelFilePath);
            var worksheet = workbook.Worksheet("SpareParts");
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // تخطي الصف الأول

            int skippedDueToExistence = 0;
            int skippedDueToInvalidData = 0;
            int addedData = 0;

            foreach(var row in rows)
            {
                var sapCodeStr = row.Cell(1).GetValue<string>();

                if(int.TryParse(sapCodeStr , out int sapCode))
                {
                    // تحقق إذا كان SapCode موجود بالفعل (يمكنك إنشاء دالة أخرى لهذه الغاية)
                    if(await CheckIfSapCodeExists(sapCode))
                    {
                        skippedDueToExistence++;
                        continue;
                    }

                    var parameters = new Dictionary<string , object>
                {
                    { "SapCode", sapCode },
                    { "PartNo", row.Cell(2).GetValue<string>() },
                    { "MatrialGroup", row.Cell(3).GetValue<string>() },
                    { "Model", row.Cell(4).GetValue<string>() },
                    { "DescriptionAR", row.Cell(5).GetValue<string>() },
                    { "DescriptionEN", row.Cell(6).GetValue<string>() },
                    { "C1", row.Cell(7).GetValue<string>() },
                    { "C2", row.Cell(8).GetValue<string>() },
                    { "IsDamaged", row.Cell(9).GetValue<bool>() }
                };

                    if(ValidateRow(parameters))
                    {
                        await InsertIntoTableAsync("SpaerPart" , parameters);
                        addedData++;
                    }
                    else
                    {
                        skippedDueToInvalidData++;
                    }
                }
                else
                {
                    skippedDueToInvalidData++;
                }
            }

            await MessageService.ShowMessage("معلومة" , $"تم أضافة ({addedData}) صفوف بنجاح،\n" +
              $"تم تخطي عدد ({skippedDueToExistence}) صفوف بسبب أن الكود موجود بالفعل،\n" +
              $"وتم تخطي عدد ({skippedDueToInvalidData}) صفوف بسبب عدم توافق البيانات." , Colors.LawnGreen);
        }
        catch(Exception ex)
        {
            await MessageService.ShowMessage($"حدث خطأ أثناء معالجة الملف: " , ex.Message , Colors.IndianRed);
            System.Diagnostics.Debug.WriteLine($"رسالة خطأ: {ex.Message}");
        }
    }

    public async Task<List<SpaerPart>> LoadPageDataAsync(string searchNameTerm , int pageNumber)
    {
        _currentPage = pageNumber;
        int offset = (_currentPage - 1) * PageSize;
        return _cacheSpaerParts.ContainsKey(searchNameTerm) ? _cacheSpaerParts[searchNameTerm].Skip(offset).Take(PageSize).ToList() : new List<SpaerPart>();
    }

    //=====================================================================================
    public async Task ImportDatabaseFileAsync()
    {
        string filePath = "";
        try
        {
            RequestPermissions();

            // طلب الصلاحيات
            bool hasPermission = await CheckAndRequestStoragePermissionAsync();
            if(!hasPermission)
            {
                await basePage.DisplayAlert("صلاحية مفقودة" , "التطبيق يحتاج إلى صلاحيات للوصول إلى ملفات WhatsApp." + "\n 'ImportDatabaseFileAsync'" , "موافق");
                return;
            }
            // 2. إذا لم يتم العثور على الملف في مسارات WhatsApp، الانتقال للبحث في جميع المسارات الثابتة
            if(string.IsNullOrEmpty(filePath))
            {
                try
                {
                    filePath = await SearchDatabaseInAllDirectoriesAsync();
                    if(string.IsNullOrEmpty(filePath))
                    {
                        await MessageService.ShowMessage("حدث خطأ غير متوقع" , "خطأ غير معالج\n" + "null: SearchDatabaseInAllDirectoriesAsync" + "\n 'ImportDatabaseFileAsync'" , Colors.IndianRed);
                        return;
                    }
                    await MessageService.ShowMessage("معلومة" , "تم العثور على مسار\n" + "filePath: " + filePath + "\n 'ImportDatabaseFileAsync'" , Colors.LawnGreen);
                }
                catch(UnauthorizedAccessException uaEx)
                {
                    await MessageService.ShowMessage("حدث خطأ غير متوقع" , "الصلاحيات غير كافية للوصول إلى الملف\n" + uaEx.Message + "\n 'ImportDatabaseFileAsync'" , Colors.IndianRed);
                }
                catch(Exception ex)
                {
                    //await basePage.DisplayAlert("خطأ غير معالج" , $"حدث خطأ غير متوقع: {ex.Message}" , "موافق");
                    await MessageService.ShowMessage("حدث خطأ غير متوقع" , "خطأ غير معالج\n" + ex.Message + "\n 'ImportDatabaseFileAsync'" , Colors.IndianRed);
                }
            }
            else
            {
                await MessageService.ShowMessage("خطأ" , "لم يتم العثور على ملف" + "\n 'ImportDatabaseFileAsync'" , Colors.IndianRed);
            }

            // 3. إذا لم يتم العثور على الملف في المسارات الثابتة، الانتقال للبحث باستخدام المتصفح
            if(string.IsNullOrEmpty(filePath))
            {
                filePath = await PromptUserToBrowseForDatabaseAsync();
            }

            // نسخ قاعدة البيانات إذا تم العثور على الملف
            if(!string.IsNullOrEmpty(filePath))
            {
                CopyDatabase(filePath , _dbPath);
            }
        }
        catch(UnauthorizedAccessException uaEx)
        {
            await MessageService.ShowMessage("حدث خطأ غير متوقع" , "الصلاحيات غير كافية للوصول إلى الملف\n" + uaEx.Message , Colors.IndianRed);
        }
        catch(Exception ex)
        {
            await MessageService.ShowMessage("حدث خطأ غير متوقع" , "خطأ غير معالج\n" + ex.Message , Colors.IndianRed);
        }
    }

    // دالة للبحث في جميع المسارات الثابتة
    private async Task<string> SearchDatabaseInAllDirectoriesAsync()
    {
        if(GetFixedPaths().Count > 0)
        {
            var LatestDatabaseFile = await GetLatestDatabaseFile(GetFixedPaths());
            if(!string.IsNullOrEmpty(LatestDatabaseFile))
            {
                await MessageService.ShowMessage("معلومة" , "تم العثور على قاعدة البيانات" + "\n" + LatestDatabaseFile + "\n" + "null: SearchDatabaseInAllDirectoriesAsync" , Colors.LawnGreen);
                return LatestDatabaseFile;
            }

            await MessageService.ShowMessage("حدث خطأ غير متوقع" , "خطأ غير معالج\n" + "null: SearchDatabaseInAllDirectoriesAsync" , Colors.IndianRed);
            return null;
        }
        else
        {
            await MessageService.ShowMessage("حدث خطأ غير متوقع" , "GetFixedPaths = 0.\n" + "null: SearchDatabaseInAllDirectoriesAsync" , Colors.IndianRed);
            return null;
        }
        return null;
    }

    // دالة للبحث في جميع المجلدات الثابتة
    private async Task<string> GetLatestDatabaseFile(List<string> paths)
    {
        string latestFilePath = null;
        try
        {
            DateTime latestFileDate = DateTime.MinValue;

            foreach(var path in paths)
            {
                if(Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path , "*.db3" , SearchOption.AllDirectories);
                    foreach(var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        if(fileInfo.LastWriteTime > latestFileDate)
                        {
                            latestFileDate = fileInfo.LastWriteTime;
                            latestFilePath = file;
                            await MessageService.ShowMessage("معلومة" , "تم اختيار احدث قاعدة بيانات" , Colors.IndianRed);
                            return latestFilePath;
                        }
                    }
                }
            }
        }
        catch(Exception ex)
        {
            await MessageService.ShowMessage("حدث خطأ غير متوقع" , "لم يتم العثور على قاعدة بيانات\n" + ex.Message , Colors.IndianRed);
            return null;
        }
        return null;
    }

    // دالة لاسترجاع مسارات WhatsApp بناءً على النظام الأساسي
    private List<string> GetFixedPaths()
    {
        var paths = new List<string>();

        if(DeviceInfo.Platform == DevicePlatform.Android)
        {
            paths.Add("/storage/emulated/0/Android/media/com.whatsapp/WhatsApp/Media/WhatsApp Documents/Sent");
            paths.Add("/storage/emulated/0/Android/media/com.whatsapp/WhatsApp/Media/WhatsApp Documents/Private");
            paths.Add("/storage/emulated/0/Android/media/com.whatsapp/WhatsApp/Media/WhatsApp Documents");
            paths.Add("/storage/emulated/0/WhatsApp/Media/WhatsApp Documents");
            paths.Add("/storage/emulated/0/Download");
            paths.Add("/storage/emulated/0/Documents");
        }
        else if(DeviceInfo.Platform == DevicePlatform.iOS)
        {
            paths.Add(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) , "Inbox"));
            paths.Add(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) , "Documents"));
        }
        else if(DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            paths.Add(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory)));
            paths.Add(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)));
        }
        return paths;
    }

    // دالة لفتح متصفح الملفات واختيار قاعدة البيانات
    private async Task<string> PromptUserToBrowseForDatabaseAsync()
    {
        try
        {
#if ANDROID
            if(DeviceInfo.Platform == DevicePlatform.Android && Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                // استخدام Storage Access Framework لفتح الملفات في Android 10 وما فوق
                var intent = new Intent(Intent.ActionOpenDocument);
                intent.AddCategory(Intent.CategoryOpenable);
                intent.SetType("*/*");
                string[] mimeTypes = { "application/octet-stream" , "application/x-sqlite3" , "application/vnd.sqlite3" };
                intent.PutExtra(Intent.ExtraMimeTypes , mimeTypes);

                var activity = Platform.CurrentActivity;
                if(activity == null)
                {
                    await basePage.DisplayAlert("Error" , "Unable to access current activity." , "OK");
                    return null;
                }

                // استخدام StartActivityForResult بطريقة مباشرة
                activity.StartActivityForResult(intent , 0);

                // هنا يجب أن تستقبل النتيجة بشكل يدوي في دالة OnActivityResult وتقوم بمعالجتها
                // استخدم طريقة بديلة أو مكتبة لإدارة النتيجة مثل MAUI Essentials أو إعادة كتابة جزء الاستجابة بشكل منفصل
            }
            else
#endif
            {
                // استخدام FilePicker للإصدارات الأخرى أو الأنظمة الأخرى
                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform , IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "public.data" } },
                        { DevicePlatform.Android, new[] { "*/*" } },
                        { DevicePlatform.WinUI, new[] { ".db3", ".sqlite" } }
                    });

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select Database File" ,
                    FileTypes = customFileType
                });

                if(result != null)
                {
                    return result.FullPath;
                }
                else
                {
                    await basePage.DisplayAlert("No File Selected" , "Please select a file." , "OK");
                }
            }
        }
        catch(Exception ex)
        {
            await basePage.DisplayAlert("Error" , $"An error occurred: {ex.Message}" , "OK");
            await MessageService.ShowMessage("حدث خطأ غير متوقع" , "الصلاحيات غير كافية للوصول إلى الملف\n" + ex.Message + "\n'PromptUserToBrowseForDatabaseAsync'" , Colors.IndianRed);
        }

        return null;
    }

    // دالة لنسخ قاعدة البيانات
    private static async void CopyDatabase(string sourcePath , string destinationPath)
    {
        try
        {
            // نسخ قاعدة البيانات مع استبدال الملف القديم
            File.Copy(sourcePath , destinationPath , true);

            // التحقق من وجود الملف بعد النسخ
            if(File.Exists(destinationPath))
            {
                // مقارنة حجم الملف للتحقق من أنه تم النسخ بنجاح
                long sourceFileSize = new FileInfo(sourcePath).Length;
                long destinationFileSize = new FileInfo(destinationPath).Length;

                if(sourceFileSize == destinationFileSize)
                {
                    Console.WriteLine("تم نسخ قاعدة البيانات الجديدة واستبدالها بالقديمة بنجاح.");
                    await MessageService.ShowMessage("معلومة" , "تم استيراد قاعدة البيانات بنجاح" + "\n'CopyDatabase'" , Colors.Green);
                }
                else
                {
                    Console.WriteLine("تحذير: حجم قاعدة البيانات المنسوخة لا يطابق المصدر.");
                    await MessageService.ShowMessage("تحذير" , "حجم قاعدة البيانات المنسوخة لا يطابق المصدر" + "\n'CopyDatabase'" , Colors.Yellow);
                }
            }
            else
            {
                Console.WriteLine("خطأ: لم يتم العثور على قاعدة البيانات في الموقع المستهدف.");
                await MessageService.ShowMessage("خطأ" , "لم يتم العثور على قاعدة البيانات في الموقع المستهدف" + "\n'CopyDatabase'" , Colors.IndianRed);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"حدث خطأ أثناء نسخ قاعدة البيانات: {ex.Message}");
            await MessageService.ShowMessage("خطأ" , "خطأ اثناء استيراد قاعدة البيانات: " + ex.Message + "\n'CopyDatabase'" , Colors.IndianRed);
        }
    }

    // دالة لحذف الملف بعد نسخه
    private async void DeleteFile(string filePath)
    {
        try
        {
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
                System.Diagnostics.Debug.WriteLine($"تم حذف الملف: {filePath}");
                await MessageService.ShowMessage("تنبيه" , $"تم حذف الملف: {filePath}" , Colors.LawnGreen);
            }
        }
        catch(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطأ أثناء حذف الملف: {ex.Message}");
            await MessageService.ShowMessage("خطأ أثناء حذف الملف" , ex.Message , Colors.IndianRed);
        }
    }
    //=====================================================================================

    // دالة للبحث مع التحميل الكسول والتخزين المؤقت
    public async Task<List<SpaerPart>> SearchDataAsync(        string searchDescriptionARTerm ,        string searchPartNoTerm ,        string searchDescriptionENTerm ,        string searchModelTerm ,        string searchCodeTerm ,       string searchGropTerm ,        string searchC1Term        )
    {
        // حساب العدد الإجمالي للسجلات لتحديد عدد الصفحات
        _totalRows = await GetTotalRowsAsync(           searchDescriptionARTerm ,           searchPartNoTerm ,           searchDescriptionENTerm ,           searchModelTerm ,           searchCodeTerm ,           searchGropTerm ,           searchC1Term            );
        _totalPages = (int)Math.Ceiling((double)_totalRows / PageSize);

        string cacheKey = $"{searchCodeTerm}|{searchModelTerm}|{searchDescriptionARTerm}|{searchDescriptionENTerm}|{searchC1Term}|{searchGropTerm}|{searchPartNoTerm}|";

        if(_cacheSpaerParts.ContainsKey(cacheKey))
        {
            if(_cacheSpaerParts[cacheKey] != null)
            {
                return _cacheSpaerParts[cacheKey].Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList();
            }
        }

        var query = _connection.Table<SpaerPart>();

        if(!string.IsNullOrWhiteSpace(searchCodeTerm))
        {
            query = query.Where(row => row.SapCode.ToString().Contains(searchCodeTerm));
        }

        if(!string.IsNullOrWhiteSpace(searchDescriptionARTerm))
        {
            query = query.Where(row => row.DescriptionAR.Contains(searchDescriptionARTerm));
        }

        if(!string.IsNullOrWhiteSpace(searchPartNoTerm))
        {
            query = query.Where(row => row.DescriptionAR.Contains(searchPartNoTerm));
        }

        if(!string.IsNullOrWhiteSpace(searchDescriptionENTerm))
        {
            query = query.Where(row => row.DescriptionAR.Contains(searchDescriptionENTerm));
        }

        if(!string.IsNullOrWhiteSpace(searchModelTerm))
        {
            query = query.Where(row => row.DescriptionAR.Contains(searchModelTerm));
        }

        if(!string.IsNullOrWhiteSpace(searchCodeTerm))
        {
            query = query.Where(row => row.DescriptionAR.Contains(searchCodeTerm));
        }

        if(!string.IsNullOrWhiteSpace(searchGropTerm))
        {
            query = query.Where(row => row.DescriptionAR.Contains(searchGropTerm));
        }

        if(!string.IsNullOrWhiteSpace(searchC1Term))
        {
            query = query.Where(row => row.DescriptionAR.Contains(searchC1Term));
        }

        var result = await query.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToListAsync();

        // تخزين النتيجة في الذاكرة المؤقتة
        _cacheSpaerParts[cacheKey] = result;

        return result;
    }

    public async Task<int> GetTotalRowsAsync(        string searchDescriptionARTerm ,        string searchPartNoTerm ,        string searchDescriptionENTerm ,        string searchModelTerm ,        string searchCodeTerm ,        string searchGropTerm ,        string searchC1Term)
    {
        var query = _connection.Table<SpaerPart>();

        if(!string.IsNullOrWhiteSpace(searchCodeTerm))
        {
            query = query.Where(row => row.SapCode.ToString().Contains(searchCodeTerm));
        }

        if(!string.IsNullOrWhiteSpace(searchDescriptionARTerm))
        {
            query = query.Where(row => row.DescriptionAR.Contains(searchDescriptionARTerm));
        }

        if(!string.IsNullOrWhiteSpace(searchPartNoTerm))
        {
            query = query.Where(row => row.DescriptionAR.Contains(searchPartNoTerm));
        }

        if(!string.IsNullOrWhiteSpace(searchDescriptionENTerm))
        {
            query = query.Where(row => row.DescriptionAR.Contains(searchDescriptionENTerm));
        }

        if(!string.IsNullOrWhiteSpace(searchModelTerm))
        {
            query = query.Where(row => row.DescriptionAR.Contains(searchModelTerm));
        }

        if(!string.IsNullOrWhiteSpace(searchCodeTerm))
        {
            query = query.Where(row => row.DescriptionAR.Contains(searchCodeTerm));
        }

        if(!string.IsNullOrWhiteSpace(searchGropTerm))
        {
            query = query.Where(row => row.DescriptionAR.Contains(searchGropTerm));
        }

        if(!string.IsNullOrWhiteSpace(searchC1Term))
        {
            query = query.Where(row => row.DescriptionAR.Contains(searchC1Term));
        }

        return await query.CountAsync();
    }
     
    public async Task<List<SpaerPart>> GetCurrentPageAsync()
    {
        return await SearchDataAsync("" , "" , "" , "" , "" , "" , "");
    }
     
    public async void ExportDatabase()
    {
        // المسار الوجهة حيث سيتم نسخ قاعدة البيانات (سطح المكتب) 
        string destPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) , DatabasefileName);

        // تحقق من وجود الملف الأصلي
        if(File.Exists(_dbPath))
        {
            // نسخ الملف إلى المسار الوجهة 
            File.Copy(_dbPath , destPath , true);
            await MessageService.ShowMessage("معلومة" , $"تم تصدير قاعدة البيانات إلى: {destPath}" , Colors.LawnGreen);
            System.Diagnostics.Debug.WriteLine($"تم تصدير قاعدة البيانات إلى: {destPath}");

            // فتح المجلد الذي يحتوي على قاعدة البيانات المنسوخة
            string folderPath = Path.GetDirectoryName(_dbPath);
            if(folderPath != null && Directory.Exists(folderPath))
            {
                // فتح المجلد باستخدام مستكشف الملفات
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = folderPath ,
                    UseShellExecute = true // استخدم مستكشف الملفات الافتراضي للنظام
                });
            }
            // استدعاء دالة أخرى (إن كانت مطلوبة)
        }
        else
        {
            await MessageService.ShowMessage("معلومة" , "لم يتم العثور على الملف." , Colors.IndianRed);
            System.Diagnostics.Debug.WriteLine("لم يتم العثور على الملف.");
        }
    }
     
    public async Task ShareDatabaseAsync()
    {
        // تصدير قاعدة البيانات

        string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) , DatabasefileName);

        // تحقق من وجود الملف
        if(File.Exists(dbPath))
        {
            var fileToShare = new ShareFile(dbPath);

            var request = new ShareFileRequest
            {
                Title = "مشاركة قاعدة البيانات" ,
                File = fileToShare
            };

            await Share.RequestAsync(request);
        }
        else
        {
            await MessageService.ShowMessage("معلومة" , "لم يتم العثور على الملف." , Colors.IndianRed);
            System.Diagnostics.Debug.WriteLine("لم يتم العثور على الملف.");
        }
    }

    // دالة لحذف جميع البيانات في الجدول
    public async Task ClearTableDataAsync()
    {
        await _connection.ExecuteAsync("DELETE FROM SpaerPart;");
        await MessageService.ShowMessage("معلومة" , "تم حذف الجدول" , Colors.LawnGreen);
    }

    // دالة لحذف الجدول من قاعدة البيانات
    public async Task DropTableAsync()
    {
        await _connection.ExecuteAsync("DROP TABLE IF EXISTS SpaerPart;");
        await MessageService.ShowMessage("معلومة" , "تم حذف جميع البيانات من الجدول" , Colors.LawnGreen);
    }

    // دالة لحذف قاعدة البيانات نهائيًا
    public void DeleteDatabase()
    {
        // تحقق من وجود قاعدة البيانات
        if(File.Exists(_dbPath))
        {
            // حذف قاعدة البيانات القديمة
            File.Delete(_dbPath);
            System.Diagnostics.Debug.WriteLine("تم حذف قاعدة البيانات القديمة: " + _dbPath);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("لم يتم العثور على قاعدة البيانات لحذفها.");
        }
    }

    // دالة لحذف ملف Excel بعد نقل البيانات منه
    public async void DeleteExcelFile()
    {
        string excelFilePath = GetFilePath();
        if(File.Exists(excelFilePath))
        {
            File.Delete(excelFilePath);
            await MessageService.ShowMessage("معلومة" , "تم حذف ملف Excel" , Colors.LawnGreen);
        }
    }
}