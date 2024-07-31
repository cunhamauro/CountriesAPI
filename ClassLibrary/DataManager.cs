using System.Data.SQLite;

namespace ClassLibrary
{
    public class DataManager
    {
        #region Paths

        const string dbPath = @"CountriesAPI_Data\Countries.sqlite";

        #endregion

        #region Atributos

        private SQLiteConnection connection;
        private SQLiteCommand command;

        #endregion

        /// <summary>
        /// Create/Open SQLite database
        /// </summary>
        public DataManager(string dbDirectory)
        {
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            try
            {
                connection = new SQLiteConnection("Data Source=" + dbPath);
                connection.Open();

                string sqlcommand = "CREATE TABLE IF NOT EXISTS Countries(countriesJson TEXT)";

                command = new SQLiteCommand(sqlcommand, connection);

                command.ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        /// <summary>
        /// Save countries in the SQLite database
        /// </summary>
        /// <param name="countriesJson">Countries Json string to save</param>
        public void SaveData(string countriesJson)
        {
            try
            {
                connection.Open();

                string sql = "INSERT INTO Countries (countriesJson)" +
                             "VALUES (@countriesJson)";

                SQLiteCommand command = new SQLiteCommand(sql, connection);

                // Parametros para guardar os dados de forma mais segura e sem erros de conversão etc
                command.Parameters.AddWithValue("@countriesJson", countriesJson);

                command.ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception e)
            {
                throw new Exception("Error saving countries in the SQLite database: " + e.Message);
            }
        }

        /// <summary>
        /// Get the countries from the SQLite database
        /// </summary>
        /// <returns>Countries Json string to deserialize</returns>
        public async Task<string> GetData(IProgress<ProgressReport> progress, int normalTaskTime, int complexTaskTime)
        {
            ProgressReport report = new ProgressReport();

            string countriesJson = "";

            #region Progress 20

            report.ProgressValue = 20;
            report.ProgressText = $"Establishing connection to the SQLite database!";
            progress.Report(report);
            await Task.Delay(normalTaskTime);

            #endregion

            try
            {
                connection.Open();

                string sql = $"SELECT countriesJson FROM Countries";

                #region Progress 30

                report.ProgressValue = 30;
                report.ProgressText = $"Reading the countries Json file from the database!";
                progress.Report(report);
                await Task.Delay(complexTaskTime);

                #endregion

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            countriesJson = reader["countriesJson"].ToString();
                        }
                    }
                }
                connection.Close();

                #region Progress 50

                report.ProgressValue = 50;
                report.ProgressText = $"Countries Json file retrieved from the database!";
                progress.Report(report);
                await Task.Delay(normalTaskTime);

                #endregion

                #region Progress 60

                report.ProgressValue = 60;
                report.ProgressText = $"Deserializing countries Json file!";
                progress.Report(report);
                await Task.Delay(complexTaskTime);

                #endregion

                return countriesJson;
            }
            catch (Exception e)
            {
                throw new Exception("Error fetching countries from the SQLite database: " + e.Message);
            }
        }

        /// <summary>
        /// Delete countries data from the SQLite database
        /// </summary>
        public void DeleteData()
        {
            try
            {
                connection.Open();

                string sql = "DELETE FROM Countries";

                command = new SQLiteCommand(sql, connection);

                command.ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception e)
            {
                throw new Exception("Error deleting countries from the SQLite database: " + e.Message);
            }
        }

        /// <summary>
        /// Download country flag images
        /// </summary>
        /// <param name="imageUrl">Country flag image URL</param>
        /// <param name="countryName">Country name</param>
        /// <returns>Task</returns>
        public async Task DownloadFlagImages(string imageUrl, string countryName, string imagesPath)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(imageUrl);
                    response.EnsureSuccessStatusCode();

                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    var imagePath = Path.Combine($"{imagesPath}", $"{countryName}.png");

                    await File.WriteAllBytesAsync(imagePath, imageBytes);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }
    }
}
