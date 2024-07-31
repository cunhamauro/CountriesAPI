using System.Net;

namespace ClassLibrary
{
    public class InternetConnection
    {
        /// <summary>
        /// Check if there is a valid Internet connection
        /// </summary>
        /// <returns>Valid Internet connection?</returns>
        public static async Task<bool> Valid()
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(1); // 1 second timeout to not take much time when there isn't a valid connection
                // Because if application was started with a connection and then turned off, it takes too much time waiting for the timeout when country selection changed

                try
                {
                    var response = await client.GetAsync("http://clients3.google.com/generate_204");
                    return response.StatusCode == HttpStatusCode.NoContent; // HTTP 204 No Content
                }
                catch (HttpRequestException)
                {
                    return false;
                }
                catch (TaskCanceledException)
                {
                    // This exception is thrown when the request times out
                    return false;
                }
            }
        }
    }
}
