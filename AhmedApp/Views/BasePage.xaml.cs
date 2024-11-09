namespace AhmedApp.Views;

public partial class BasePage : ContentPage
{
    public BasePage()
    {
        InitializeComponent();
    }

    private void ContentPage_Loaded(object sender , EventArgs e)
    {
        //var content = Application.Current.Windows.OfType<MainPage>().FirstOrDefault()  ;

        // عرض الصفحة الافتراضية عند التحميل
        SetContent(new MainPage()); // استبدل MainPage بالصفحة الافتراضية التي ترغب في عرضها
    }

    public void SetContent(View content)
    {
        // تعيين المحتوى للـ ContentPresenter
        ContentPresenter.Content = content;

        // تحقق مما إذا كانت المحتويات هي SetingsWindow
        if(content is MainPage)
        {
            BtnBack.IsVisible = false;
        }
        else
        {
            BtnBack.IsVisible = true;
        }
    }

    //public void SharedMessageLabel(string Titel , string Message)
    //{
    //    MessageLabel.Text = Titel + ": " + Message;
    //}

    //public async Task ShowAlert(string title , string message , string cancel)
    //{
    //    await this.DisplayAlert(title , message , cancel);
    //}

    //public async Task<bool> ShowAlert(string title , string message , string accept , string cancel)
    //{
    //    bool answer = await this.DisplayAlert(title , message , accept , cancel);
    //    return answer;
    //}
     
    private  void GoNextPage(object sender , EventArgs e)
    {
        // استخدم الصفحات المراد فتحها
        var nextPage = new SetingsWindow(); // أو أي صفحة تريد فتحها
        SetContent(nextPage);
        // await DisplayAlert("info" , "Navigating to the next page" , "OK");
    }

    private  void GoBackPage(object sender , EventArgs e)
    {       // هنا يمكنك استخدام أي منطق لتحديد الصفحة السابقة
        var previousPage = new MainPage(); // أو أي صفحة ترغب في الرجوع إليها
        SetContent(previousPage);
        //await DisplayAlert("info" , "Returning to the previous page" , "OK");
    }

    protected override bool OnBackButtonPressed()
    {
        // استدعاء دالة الرجوع عند ضغط زر الرجوع المدمج
        GoBackPage(this , EventArgs.Empty);

        // إعادة true لمنع التطبيق من العودة بشكل تلقائي
        return true;
    }
     
    private void OnBorderDoubleTapped(object sender , TappedEventArgs e)
    {
        // فتح نافذة صغيرة لعرض الرسائل كاملة
        var popupPage = new PopupMessagesPage();
        Application.Current.MainPage.Navigation.PushModalAsync(popupPage);
    } 
}