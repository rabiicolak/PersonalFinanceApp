namespace PersonalFinance.Maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new Views.LoginPage()); MainPage = new NavigationPage(new Views.LoginPage());
        }
    }
}
