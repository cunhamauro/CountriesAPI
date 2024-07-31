global using Microsoft.Web.WebView2.Core;
global using System.ComponentModel;
global using System.Data;
global using System.Windows;
global using System.Windows.Documents;
using Syncfusion.Licensing;


namespace ProjetoCountriesAPI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Register Syncfusion license
            SyncfusionLicenseProvider.RegisterLicense("MzM3ODY1NEAzMjM2MmUzMDJlMzBoenZKWlVqcXQrOE0rL2JnUUFEcGdJYjRHa2VyWXdYck5rUzNEcWlVMXk0PQ==");
            base.OnStartup(e);
        }
    }

}
