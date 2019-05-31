using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OsuBeatmapRequest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void Event_GetAccessToken(object sender, RoutedEventArgs e)
        {
            GetFacebookAuthWindow getFacebookAuth = new GetFacebookAuthWindow();
            getFacebookAuth.browser.Navigate("https://www.facebook.com/v3.3/dialog/oauth?client_id=485108851991595&redirect_uri=https://google.com&display=popup&response_type=token");
            getFacebookAuth.ShowDialog();
            var url = getFacebookAuth.browser.Source.ToString();
            if (url.Contains("https://www.google.com/#access_token="))
            {
                var trimStart = url.Substring(url.IndexOf("=") + 1);
                var accesstoken = trimStart.Substring(0, trimStart.IndexOf("&"));
                txbAccessToken.Text = accesstoken;
            }


        }

        private void Event_Initialize(object sender, RoutedEventArgs e)
        {
            PresentatationWindow window = new PresentatationWindow();
            window.FBAccess = txbAccessToken.Text;
            window.VideoID = txbVideoID.Text;
            window.Username = txbUsername.Text;
            window.Password = txbPassword.Text;
            if(!string.IsNullOrEmpty(txbInterval.Text))
                window.Interval = int.Parse(txbInterval.Text);
            window.InitBot();
            if(window.CanShow)
                window.Show();
        }
    }
}
