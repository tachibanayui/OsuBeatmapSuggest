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
using System.Windows.Shapes;

namespace OsuBeatmapRequest
{
    /// <summary>
    /// Interaction logic for GetFacebookAuthWindow.xaml
    /// </summary>
    public partial class GetFacebookAuthWindow : Window
    {
        public GetFacebookAuthWindow()
        {
            InitializeComponent();

            browser.Navigated += (s, e) =>
            {
                if (browser.Source.ToString().Contains("https://www.google.com/#access_token="))
                    Close();
            };
        }
    }
}
