using AhmedApp.Helper;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace AhmedApp.Views
{
    public partial class MainPage : ContentView
    { 
        public MainPage()
        {
            InitializeComponent();
            CheckDarkMode();
            ThemeHelper.KeepScreenOn(); 
        }

        private void CheckDarkMode()
        {
            bool isDarkMode = ThemeHelper.IsDarkMode();
            UpdateCheckBox(isDarkMode);
        }

        private void UpdateCheckBox(bool isDarkMode)
        {
            //darkModeCheckBox.IsChecked = isDarkMode;
            //statusLabel.Text = isDarkMode ? "ON" : "OFF";
            //statusLabel.TextColor = isDarkMode ? Colors.Lime : Colors.Red;
            //darkModeCheckBox.Color = isDarkMode ? Colors.Green : Colors.Gray;
        }

        private void OnCheckBoxCheckedChanged(object sender , CheckedChangedEventArgs e)
        {
            UpdateCheckBox(e.Value);
            if(e.Value)
            {
                Application.Current.UserAppTheme = AppTheme.Dark;
            }
            else
            {
                Application.Current.UserAppTheme = AppTheme.Light;
            }
        }

        private void OnLabelTapped(object sender , EventArgs e)
        {
            // تغيير حالة الوضع بين تشغيل وإيقاف
            //if(statusLabel.Text == "ON")
            //{
            //    statusLabel.Text = "OFF";
            //    statusLabel.TextColor = Colors.Red;
            //    darkModeCheckBox.IsChecked = false; // قم بتحديث حالة الـ Switch هنا إذا لزم الأمر
            //}
            //else
            //{
            //    statusLabel.Text = "ON";
            //    statusLabel.TextColor = Colors.Lime;
            //    darkModeCheckBox.IsChecked = true; // قم بتحديث حالة الـ Switch هنا إذا لزم الأمر
            //}
        }

        private void OpenSpaerPart(object sender , EventArgs e)
        {
            // احصل على المثيل الحالي من BasePage
            var basePage = Application.Current.MainPage as BasePage;
            // قم بتعيين المحتوى للصفحة
            basePage?.SetContent(new SpaerPartsPage());
        }

        private void OpenCopyToExcel(object sender , EventArgs e)
        {
            // احصل على المثيل الحالي من BasePage
            var basePage = Application.Current.MainPage as BasePage;

            // تعيين الصفحة الجديدة بداخل BasePage
            var newPage = new SetingsWindow();
            basePage?.SetContent(newPage); // تعيين الصفحة الجديدة في ContentPresenter
        } 

        //var content = Application.Current.Windows.OfType<SetingsWindow>().FirstOrDefault();
        //BasePage basePage = new();
        //basePage.SetContent(content);
        //await Navigation.PushAsync(new SetingsWindow());

    }
}