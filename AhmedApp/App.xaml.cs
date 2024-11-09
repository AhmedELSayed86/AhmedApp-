using AhmedApp.Views;

namespace AhmedApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new BasePage();
        } 
    }
}
