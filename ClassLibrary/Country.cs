namespace ClassLibrary
{
    public class Country
    {
        #region Properties

        /// <summary>
        /// Country name
        /// </summary>
        public CountryName Name { get; set; } = new CountryName();

        /// <summary>
        /// Country capital(s)
        /// </summary>
        public List<string> Capital { get; set; } = new List<string>();

        /// <summary>
        /// Country region
        /// </summary>
        public string Region { get; set; } = "N/A";

        /// <summary>
        /// Country sub region
        /// </summary>
        public string SubRegion { get; set; } = "N/A";

        /// <summary>
        /// Country area
        /// </summary>
        public double? Area { get; set; } // Nullable double to correctly display N/A instead of default value 0 when null

        /// <summary>
        /// Country population
        /// </summary>
        public int? Population { get; set; }  // Nullable int to correctly display N/A instead of default value 0 when null

        /// <summary>
        /// Country GINI coefficient
        /// </summary>
        public Dictionary<string, double>? Gini { get; set; }  // Nullable dictionary to correctly display N/A instead of default value 0 when null

        /// <summary>
        /// Country flag image link
        /// </summary>
        public Flag Flags { get; set; } = new Flag();

        /// <summary>
        /// Country languages
        /// </summary>
        public Dictionary<string, string> Languages { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Two letter country code assigned by ISO
        /// </summary>
        public string CCA2 { get; set; } = "N/A";

        /// <summary>
        /// Independency status of the country
        /// </summary>
        public bool? Independent { get; set; } // Nullable bool to correctly display N/A instead of default value false when null

        /// <summary>
        /// Country currency
        /// </summary>
        public Dictionary<string, Currency> Currencies { get; set; } = new Dictionary<string, Currency>();

        /// <summary>
        /// Country latitude and longitude
        /// </summary>
        public List<double?> LatLng { get; set; } = new List<double?>(); // Nullable list of doubles to correctly display N/A instead of default values 0.00 when null

        /// <summary>
        /// Countries that border this country
        /// </summary>
        public List<string> Borders { get; set; } = new List<string>();

        /// <summary>
        /// Continents the country belongs to
        /// </summary>
        public List<string> Continents { get; set; } = new List<string>();

        /// <summary>
        /// TimeZones the country has
        /// </summary>
        public List<string> TimeZones { get; set; } = new List<string>();

        #endregion

        #region Readonly Output Properties

        /// <summary>
        /// Display capitals
        /// </summary>
        public string OutputCapital => Capital.Any() ? string.Join("\n", Capital) + "\n" : "N/A\n";

        /// <summary>
        /// Display country area
        /// </summary>
        public string OutputArea => Area.HasValue ? $"{Area} Km²\n" : "N/A\n";

        /// <summary>
        /// Display country independency status
        /// </summary>
        public string OutputIndependent => Independent.HasValue ? (Independent.Value ? "Yes\n" : "No\n") : "N/A\n";

        /// <summary>
        /// Display country population
        /// </summary>
        public string OutputPopulation => Population.HasValue ? $"{Population}\n" : "N/A\n";

        /// <summary>
        /// Display country GINI year and index
        /// </summary>
        public string OutputGini => Gini != null ? string.Join("\n", Gini.Select(g => $"[{g.Key}] {g.Value}%")) : "N/A";

        /// <summary>
        /// Display country languages
        /// </summary>
        public string OutputLanguages => Languages.Any() ? string.Join("\n", Languages.Values) + "\n" : "N/A\n";

        /// <summary>
        /// Display country currencies
        /// </summary>
        public string OutputCurrencies => Currencies.Any() ? string.Join("\n", Currencies.Values.Select(currency => $"[{currency.Symbol}] - {currency.Name}")) + "\n" : "N/A\n";

        /// <summary>
        /// Display country continents
        /// </summary>
        public string OutputContinents => string.Join("\n", Continents) + "\n";

        /// <summary>
        /// Display country latitude and longitude
        /// </summary>
        public string OutputLatLng(int latLng) => LatLng[latLng].HasValue ? $"{LatLng[latLng]}" : "N/A";

        /// <summary>
        /// Display country borders
        /// </summary>
        public string OutputBorders => Borders.Any() ? string.Join("\n", Borders) + "\n" : "None \n";

        #endregion

        #region Methods

        /// <summary>
        /// Display country local times
        /// </summary>
        /// <returns></returns>
        public string OutputLocalTimes()
        {
            string strLocalTimes = "";

            if (TimeZones.Any())
            {
                foreach (var timeZone in TimeZones)
                {
                    DateTime localTime = DateTime.UtcNow;

                    // Check if timezone is in daylight saving (summer)
                    if (TimeZoneInfo.Local.IsDaylightSavingTime(localTime))
                    {
                        localTime = localTime.AddHours(1); // Add 1 hour for summer time
                    }

                    if (timeZone.Length > 3)
                    {
                        string rawOffset = timeZone.Substring(3);
                        string offset = rawOffset.Substring(1);
                        TimeSpan timeOffset = TimeSpan.Parse(offset);

                        if (rawOffset[0] == '+')
                        {
                            localTime += timeOffset;
                        }
                        else if (rawOffset[0] == '-')
                        {
                            localTime -= timeOffset;
                        }
                        strLocalTimes += $"[{timeZone}] {localTime.ToString("dd-MM-yyy HH:mm:ss")} \n";
                    }
                    else
                    {
                        strLocalTimes += $"[{timeZone}] {localTime.ToString("dd-MM-yyy HH:mm:ss")} \n";
                    }
                }
            }
            else
            {
                strLocalTimes += "N/A\n";
            }

            return strLocalTimes;
        }

        /// <summary>
        /// ToString() override to display country in listbox
        /// </summary>
        /// <returns>Country code and name</returns>
        public override string ToString()
        {
            return $"{Name.Common}";
        }

        #endregion
    }
    #region Subclasses

    /// <summary>
    /// Country name class
    /// </summary>
    public class CountryName
    {
        /// <summary>
        /// Country common name
        /// </summary>
        public string Common { get; set; } = "N/A";
    }

    /// <summary>
    /// Country flag class
    /// </summary>
    public class Flag
    {
        /// <summary>
        /// Country PNG image flag link
        /// </summary>
        public string Png { get; set; } = "https://i.imgur.com/jBbF1hj.png";
    }

    /// <summary>
    /// Country currencies class
    /// </summary>
    public class Currency
    {
        /// <summary>
        /// Country currency name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Country currency symbol
        /// </summary>
        public string Symbol { get; set; }
    }

    #endregion
}
