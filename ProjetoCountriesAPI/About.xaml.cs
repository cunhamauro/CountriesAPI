using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ProjetoCountriesAPI
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        #region Consts

        const string linkedinUrl = "https://www.linkedin.com/in/cunhamauro/";
        const string githubUrl = "https://github.com/cunhamauro";
        const string version = "Version 1.0.0";
        const string year = "2024";

        #endregion

        public About()
        {
            InitializeComponent();

            LabelVersion.Content = version;
            LabelYear.Content = year;
        }

        /// <summary>
        /// Open hyperlinks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageSocials_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image imageClicked = sender as Image;
            string imageName = imageClicked.Name;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = imageName == "ImageLinkedin" ? linkedinUrl : githubUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading hyperlink!");
            }
        }
    }
}
