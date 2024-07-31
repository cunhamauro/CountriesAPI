using ClassLibrary;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Security.Policy;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAnimatedGif;

namespace ProjetoCountriesAPI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Task delay times

        const int normalTaskTime = 250;
        const int complexTaskTime = 500;

        #endregion

        #region Atributes

        List<Country> listCountries;

        DataManager dataManager;

        DispatcherTimer refreshTimer;

        static About _aboutWindow; // Static reference to track the About window

        #endregion

        #region Paths

        const string apiUrl = "https://restcountries.com";
        const string apiController = "/v3.1/all";
        const string dbDirectory = "CountriesAPI_Data";
        const string imagesPath = "CountriesAPI_FlagImages";

        #endregion

        public MainWindow()
        {
            // Initialize list of countries
            listCountries = new List<Country>();

            InitializeComponent();
        }

        /// <summary>
        /// Check if the execution is invalid (No Internet and no database)
        /// </summary>
        /// <returns>Task</returns>
        public async Task<bool> CheckInvalidExecution()
        {
            if (!Directory.Exists(dbDirectory) && !await InternetConnection.Valid())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Initialization of the data manager instance
        /// </summary>
        private void InitializeDataManager()
        {
            try
            {
                dataManager = new DataManager(dbDirectory);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error loading Countries SQLite database!");
            }
        }

        /// <summary>
        /// Disable webview with no Internet connection
        /// </summary>
        private async Task CollapseWebView()
        {
            if (!await InternetConnection.Valid())
            {
                WebViewMap.Visibility = Visibility.Collapsed; // Collapse the Google Maps webview control
            }
        }

        /// <summary>
        /// Load the list of countries
        /// </summary>
        async void LoadCountries(IProgress<ProgressReport> progress)
        {
            ProgressReport report = new ProgressReport();
            bool internetConnection = await InternetConnection.Valid();

            #region Progress 10

            report.ProgressValue = 10;
            report.ProgressText = "Checking Internet connection!";
            progress.Report(report);
            await Task.Delay(normalTaskTime);

            #endregion

            if (internetConnection) // If there is an Internet connection
            {
                // Fetch from API
                listCountries = await GetCountries(progress);

                #region Progress 70

                report.ProgressValue = 70;
                report.ProgressText = $"Countries successfully deserialized!";
                progress.Report(report);
                await Task.Delay(normalTaskTime);

                #endregion

            }
            else // If there is not an Internet connection
            {
                // Get the countries from the SQLite database and deserialize the Json string
                listCountries = DeserializeJson(await dataManager.GetData(progress, normalTaskTime, complexTaskTime));

                #region Progress 70

                report.ProgressValue = 70;
                report.ProgressText = $"Countries successfully deserialized!";
                progress.Report(report);
                await Task.Delay(normalTaskTime);

                #endregion
            }

            if (listCountries == null || listCountries.Count == 0)
            {
                if (internetConnection)
                {
                    LabelProgressCountries.Content = "No countries were fetched from the API!" + Environment.NewLine + "Please try again later!";
                }
                return;
            }

            SetControls(); // Show the controls in the window

            // Order the list of countries by name
            listCountries = listCountries.OrderBy(c => c.Name.Common).ToList(); // Also activates listbox item search by writing
            ListBoxCountries.ItemsSource = listCountries;

            DateTime finishedTime = DateTime.Now; // Time the countries finished to load
            int numCountries = listCountries.Count; // Number of countries fetched from API

            // Select a random country at the start so no empty places are shown
            SelectRandomCountry();

            // Dynamic message according to the source of the data
            if (internetConnection) // From API
            {
                #region Progress flags variables

                int totalProgress = 100 - report.ProgressValue; // Total progress to be distributed to the flags download
                double progressPerFlag = (double)totalProgress / numCountries; // Progress per flag
                double currentProgress = report.ProgressValue; // Total acumulated progress

                #endregion

                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                foreach (Country country in listCountries)
                {
                    try
                    {
                        // Download the flags image
                        await dataManager.DownloadFlagImages(country.Flags.Png, country.Name.Common, imagesPath);

                        #region Progress flag download

                        report.ProgressText = $"{numCountries} countries fetched from API {apiUrl}\nat {finishedTime}" + $"\nUpdating and saving {country.Name.Common} flag...";
                        currentProgress += progressPerFlag;
                        report.ProgressValue = (int)Math.Round(currentProgress);
                        progress.Report(report);

                        #endregion

                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, $"Error saving {country.Name.Common} flag!");
                    }
                }

                #region Progress 100

                report.ProgressValue = 100;
                report.ProgressText = $"{numCountries} countries fetched from API {apiUrl}\nat {DateTime.Now}";
                progress.Report(report);

                #endregion
            }
            else // From SQLite database
            {
                LoadLocalGif();

                #region Progress 100

                report.ProgressValue = 100;
                report.ProgressText = $"{numCountries} countries loaded from SQLite database Countries\nat {DateTime.Now}";
                progress.Report(report);

                #endregion
            }
        }

        /// <summary>
        /// Set window controls
        /// </summary>
        public void SetControls()
        {
            LabelLoading.Visibility = Visibility.Collapsed;
            LabelPlanet.Visibility = Visibility.Collapsed;
            ScrollViewer.Visibility = Visibility.Visible;
            ImageRandomCountry.Visibility = Visibility.Visible;
            ListBoxCountries.Visibility = Visibility.Visible;
            LabelBackFilter.Visibility = Visibility.Visible;
            LabelFilterContinent.Visibility = Visibility.Visible;
            ComboBoxFilterContinent.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Deserialize the json string into a list of countries
        /// </summary>
        /// <param name="countriesJson">Countries Json string to deserialize</param>
        /// <returns>Deserialized list of countries</returns>
        public List<Country> DeserializeJson(string countriesJson)
        {
            List<Country> countries = JsonConvert.DeserializeObject<List<Country>>(countriesJson);

            return countries;
        }

        /// <summary>
        /// Fetch the countries from the API
        /// </summary>
        /// <returns>List of countries</returns>
        async Task<List<Country>> GetCountries(IProgress<ProgressReport> progress)
        {
            ProgressReport report = new ProgressReport();

            #region Progress 20

            report.ProgressValue = 20;
            report.ProgressText = "Establishing an online connection!";
            progress.Report(report);
            await Task.Delay(normalTaskTime);

            #endregion

            using (var client = new HttpClient())
            {
                try
                {
                    #region Progress 30

                    report.ProgressValue = 30;
                    report.ProgressText = $"Fetching countries from the API!";
                    progress.Report(report);
                    await Task.Delay(complexTaskTime);

                    #endregion

                    client.BaseAddress = new Uri(apiUrl);
                    var response = await client.GetAsync(apiController);

                    var result = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Countries fetch from the API {apiUrl} was unsuccessfull!", "Error fetching data!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }

                    #region Progress 40

                    report.ProgressValue = 40;
                    report.ProgressText = $"Countries successfully fetched from the API!";
                    progress.Report(report);
                    await Task.Delay(normalTaskTime);

                    #endregion

                    #region Progress 50

                    report.ProgressValue = 50;
                    report.ProgressText = $"Deleting and saving countries in SQLite database!";
                    progress.Report(report);
                    await Task.Delay(complexTaskTime);

                    #endregion

                    dataManager.DeleteData(); // Clean the countries SQLite database

                    dataManager.SaveData(result); // Save the countries json string fetched from the API to the SQLite database

                    #region Progress 60

                    report.ProgressValue = 60;
                    report.ProgressText = $"Deserializing countries Json file!";
                    progress.Report(report);
                    await Task.Delay(complexTaskTime);

                    #endregion

                    return DeserializeJson(result);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return null;
                }
            }
        }

        /// <summary>
        /// Show country data when selected in listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ListBoxCountries_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Country country = ListBoxCountries.SelectedItem as Country;

            bool internetConnection = await InternetConnection.Valid(); // Check Internet connection for each selection (because Internet can go off while the program is already executing)

            if (country != null)
            {
                OutputCountryData(country); // Show the country data in the textblock

                if (refreshTimer == null)
                {
                    StartTimer(); // Start timer to refresh the country local times
                }

                string missingFlagPath = @"/Resources\404-flag.png";

                if (internetConnection) // If there is an Internet connection
                {

                    if (!string.IsNullOrWhiteSpace(country.Flags.Png)) // If the country has a flag
                    {
                        try // Load it
                        {
                            ImageCountryFlag.Source = new BitmapImage(new Uri(country.Flags.Png));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex.Message}" + Environment.NewLine + $"Error loading {country.Name.Common}'s flag!");

                            try
                            {
                                ImageCountryFlag.Source = new BitmapImage(new Uri(missingFlagPath, UriKind.Relative)); // Load local image: "404 Flag not found"
                            }
                            catch (Exception ex2)
                            {
                                MessageBox.Show($"{ex2.Message}" + Environment.NewLine + $"Error loading image [404 Flag not found]");
                            }
                        }
                    }
                    else // If it doesnt have a flag saved
                    {
                        try
                        {
                            ImageCountryFlag.Source = new BitmapImage(new Uri(missingFlagPath, UriKind.Relative)); // Load local image: "404 Flag not found"
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex.Message}" + Environment.NewLine + $"Error loading image [404 Flag not found]");
                        }
                    }

                    await LoadGoogleMap(country.Name.Common);
                }
                // If there isn't an Internet connection
                else
                {
                    string relativeFlagPath = Path.Combine(imagesPath, $"{country.Name.Common}.png"); // Get the relative flag path
                    string fullFlagPath = Path.GetFullPath(relativeFlagPath); // Get the full path of the image because it doesnt have a build action of resource (meaning relative paths can't be used)

                    if (File.Exists(fullFlagPath)) // If the country has an image flag saved
                    {
                        try
                        {
                            ImageCountryFlag.Source = new BitmapImage(new Uri(fullFlagPath));
                        }
                        catch (Exception)
                        {
                            try
                            {
                                ImageCountryFlag.Source = new BitmapImage(new Uri(missingFlagPath, UriKind.Relative)); // Load local image: "404 Flag not found"
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"{ex.Message}" + Environment.NewLine + $"Error loading image [404 Flag not found]");
                            }
                        }
                    }
                    else // If the country has no flag
                    {
                        try
                        {
                            ImageCountryFlag.Source = new BitmapImage(new Uri(missingFlagPath, UriKind.Relative)); // Load local image: "404 Flag not found"
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex.Message}" + Environment.NewLine + $"Error loading image [404 Flag not found]");
                        }
                    }

                    // Set the webview and load the local gif in place of the map
                    if (ImageNoMaps.Visibility != Visibility.Visible)
                    {
                        WebViewMap.Visibility = Visibility.Collapsed;
                        LoadLocalGif();
                    }
                }
            }
        }

        /// <summary>
        /// Load a local gif in the place of Google Maps
        /// </summary>
        private void LoadLocalGif()
        {
            ImageNoMaps.Visibility = Visibility.Visible;
            try
            {
                string gifPath = @"/Resources\rotating-earth.gif";
                var gif = new BitmapImage(new Uri(gifPath, UriKind.Relative));
                ImageBehavior.SetAnimatedSource(ImageNoMaps, gif);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error loading gif!");
            }
        }

        /// <summary>
        /// Clean and output country data
        /// </summary>
        /// <param name="country">Selected country</param>
        private void OutputCountryData(Country country)
        {
            TextBlockCountryData.Text = string.Empty;

            TextBlockCountryData.Inlines.Add(new Bold(new Run($"[{country.CCA2}] - {country.Name.Common}\n\n") { FontSize = 18 }));

            TextBlockCountryData.Inlines.Add(new Bold(new Run($"{Pluralize("Capital", country.Capital == null ? 0 : country.Capital.Count)}:\n")));
            TextBlockCountryData.Inlines.Add(new Run($"{country.OutputCapital}\n"));

            TextBlockCountryData.Inlines.Add(new Bold(new Run($"{Pluralize("Language", country.Languages.Count)}:\n")));
            TextBlockCountryData.Inlines.Add(new Run($"{country.OutputLanguages}\n"));

            TextBlockCountryData.Inlines.Add(new Bold(new Run($"{Pluralize("Currency", country.Currencies.Count)}:\n")));
            TextBlockCountryData.Inlines.Add(new Run($"{country.OutputCurrencies}\n"));

            TextBlockCountryData.Inlines.Add(new Bold(new Run($"{Pluralize("Local time", country.TimeZones.Count)}:\n")));
            TextBlockCountryData.Inlines.Add(new Run($"{country.OutputLocalTimes()}\n"));

            TextBlockCountryData.Inlines.Add(new Bold(new Run($"Area: ")));
            TextBlockCountryData.Inlines.Add(new Run($"{country.OutputArea}\n"));

            TextBlockCountryData.Inlines.Add(new Bold(new Run($"Population: ")));
            TextBlockCountryData.Inlines.Add(new Run($"{country.OutputPopulation}\n"));

            TextBlockCountryData.Inlines.Add(new Bold(new Run($"{Pluralize("Continent", country.Continents.Count)}:\n")));
            TextBlockCountryData.Inlines.Add(new Run($"{country.OutputContinents}\n"));

            TextBlockCountryData.Inlines.Add(new Bold(new Run($"Region:\n")));
            TextBlockCountryData.Inlines.Add(new Run($"{country.Region}\n\n"));

            TextBlockCountryData.Inlines.Add(new Bold(new Run($"Sub region:\n")));
            TextBlockCountryData.Inlines.Add(new Run($"{country.SubRegion}\n\n"));

            TextBlockCountryData.Inlines.Add(new Bold(new Run($"Latitude: ")));
            TextBlockCountryData.Inlines.Add(new Run($"{country.OutputLatLng(0)}\n"));
            TextBlockCountryData.Inlines.Add(new Bold(new Run($"Longitude: ")));
            TextBlockCountryData.Inlines.Add(new Run($"{country.OutputLatLng(1)}\n\n"));

            TextBlockCountryData.Inlines.Add(new Bold(new Run($"Borders:\n")));
            TextBlockCountryData.Inlines.Add(new Run($"{country.OutputBorders}\n"));

            TextBlockCountryData.Inlines.Add(new Bold(new Run($"Independent:\n")));
            TextBlockCountryData.Inlines.Add(new Run($"{country.OutputIndependent}\n"));

            TextBlockCountryData.Inlines.Add(new Bold(new Run($"GINI Index:\n")));
            TextBlockCountryData.Inlines.Add(new Run($"{country.OutputGini}"));
        }

        /// <summary>
        /// Load Google Map Embed API with the selected country location
        /// </summary>
        /// <param name="countryName">Name of the country to show</param>
        /// <returns>Task</returns>
        private async Task LoadGoogleMap(string countryName)
        {
            string apiKey = "API_KEY_HERE"; // (Replace with your Google Maps Embed API Key)

            // HTML content with an iframe pointing to Google Maps (Google Maps Embed API only works with iframe)
            string iframeHtml = $@"
             <html>
                <body style='margin:0px; padding:0px;'>
                    <iframe 
                        width='100%' 
                        height='100%' 
                        frameborder='0' 
                        style='border:0;' 
                        src='https://www.google.com/maps/embed/v1/search?key={apiKey}&q={countryName}&language=en'
                        allowfullscreen>
                    </iframe>
                </body>
             </html>";

            try
            {
                // Await that the web view is loaded
                await WebViewMap.EnsureCoreWebView2Async(null);

                // Load the HTML content into the web view
                WebViewMap.NavigateToString(iframeHtml);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error loading Google Maps API!");
            }
        }

        /// <summary>
        /// Clear webview data
        /// </summary>
        /// <returns>Task</returns>
        private async Task ClearWebViewData()
        {
            try
            {
                if (WebViewMap.CoreWebView2 != null)
                {
                    var profile = WebViewMap.CoreWebView2.Profile;

                    // Clear the webview stored files because otherwise it is possible that it triggers a Windows error
                    // File path name too long because the generated stored files/archives have long names
                    // It prevents the projects folder from being moved or zipped (Windows has a max path length of 256 characters)
                    // Bonus: It makes the project lighter
                    await profile.ClearBrowsingDataAsync(CoreWebView2BrowsingDataKinds.AllDomStorage);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error clearing WebView2 storage cache!");
            }
        }

        /// <summary>
        /// Format words to plural if value counts > 1
        /// </summary>
        /// <param name="text">Text to format</param>
        /// <param name="count">Value to analyze</param>
        /// <returns>Formated text</returns>
        private string Pluralize(string text, int count)
        {
            if (count > 1)
            {
                if (text == "Currency") // Currency is a different case because Currency -> Currencies (Not currencys)
                {
                    return "Currencies";
                }
                return text + "s";
            }

            return text;
        }

        /// <summary>
        /// Select a random country to display
        /// </summary>
        private void SelectRandomCountry()
        {
            Random random = new Random();
            if (ListBoxCountries.Items.Count > 0)
            {
                int randomCountryIndex = random.Next(ListBoxCountries.Items.Count);
                ListBoxCountries.SelectedIndex = randomCountryIndex;

                ListBoxCountries.ScrollIntoView(ListBoxCountries.Items[randomCountryIndex]); // Scroll to the selected country
                ListBoxCountries.Focus(); // And focus it (blue select instead of greyed out)
            }
        }

        /// <summary>
        /// Refresh country data with each timer tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            Country country = ListBoxCountries.SelectedItem as Country;

            if (country != null)
            {
                // Update country data (so local time clocks update by each second)
                OutputCountryData(country);
            }
        }

        /// <summary>
        /// Click to get a random country
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageRandomCountry_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectRandomCountry();
        }

        /// <summary>
        /// After the window is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (await CheckInvalidExecution()) // If the execution is invalid
            {
                LabelLoading.Visibility = Visibility.Visible;
                LabelMessage.Visibility = Visibility.Visible;
                LabelLoading.Content = "INVALID EXECUTION";

                return;
            }
            // If the execution is valid (Internet connection or stored data)

            LabelLoading.Visibility = Visibility.Visible;
            LabelPlanet.Visibility = Visibility.Visible;

            InitializeDataManager();

            await CollapseWebView();

            ProgressBarCountries.Visibility = Visibility.Visible;
            ProgressBarCountries.Progress = 0;

            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;

            LoadCountries(progress); // Start loading the countries and report the progress
        }

        /// <summary>
        /// In real time report of the progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReportProgress(object sender, ProgressReport e)
        {
            ProgressBarCountries.Progress = e.ProgressValue;
            LabelProgressCountries.Content = e.ProgressText;
        }

        /// <summary>
        /// Start timer to refresh local times
        /// </summary>
        private void StartTimer()
        {
            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(1);
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
        }

        /// <summary>
        /// When the form is closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Countries_Closing(object sender, CancelEventArgs e)
        {
            if (_aboutWindow != null)
            {
                _aboutWindow.Close(); // When the main window closes, also close about window if opened
            }
            await ClearWebViewData(); // Clear the webview stored data
        }

        /// <summary>
        /// Filter the list of countries by continent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxFilterContinent_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = ComboBoxFilterContinent.SelectedItem as ComboBoxItem;

            if (selectedItem != null)
            {
                string parameter = selectedItem.Content.ToString();

                List<Country> filteredList = new List<Country>();

                if (parameter == "All")
                {
                    // Show all countries ordered by name
                    filteredList = listCountries.OrderBy(c => c.Name.Common).ToList();
                }
                else
                {
                    // Filter countries by selected continent
                    filteredList = listCountries.Where(c => c.Continents.Contains(parameter)).OrderBy(c => c.Name.Common).ToList();
                }

                // Update ListBoxCountries
                ListBoxCountries.ItemsSource = filteredList;
            }
        }

        /// <summary>
        /// Open About window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageInfo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_aboutWindow == null || !_aboutWindow.IsVisible) // If the window is not opened
            {
                _aboutWindow = new About();
                _aboutWindow.Show();
            }
            else
            {
                _aboutWindow.Activate(); // Bring the window to the front
            }
        }
    }
}