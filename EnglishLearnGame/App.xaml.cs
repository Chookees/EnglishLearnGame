using System.Configuration;
using System.Data;
using System.Windows;

namespace EnglishLearnGame
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Starte den Welcome Screen
            WelcomeScreen welcomeScreen = new WelcomeScreen();
            welcomeScreen.Show();
        }
    }
}
