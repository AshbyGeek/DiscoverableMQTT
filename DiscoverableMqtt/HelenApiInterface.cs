using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace DiscoverableMqtt
{
    public interface IHelenApiInterface
    {
        /// <summary>
        /// Gets the broker URL from Helen's WebAPI server
        /// </summary>
        /// <param name="defaultValue">The value to return if Helen's API can't be reached.</param>
        /// <returns></returns>
        string GetBrokerUrl(string defaultValue);


        /// <summary>
        /// Gets the API identifier from Helen's WebAPI server
        /// </summary>
        /// <param name="name">The name to register</param>
        /// <param name="defaultValue">The value to return if Helen's API can't be reached</param>
        /// <returns></returns>
        int GetApiId(string name, int defaultValue);

        /// <summary>
        /// Creates a data packet that Helen's MQTTManager can understand
        /// </summary>
        /// <param name="apiId">The API identifier.</param>
        /// <param name="data">The data to be sent.</param>
        /// <returns></returns>
        string CreateDataMessage(int apiId, string data);

        /// <summary>
        /// Creates a location packet that Helen's MQTTManager can understand
        /// </summary>
        /// <param name="apiId">The API identifier.</param>
        /// <param name="deviceLocation">The device location.</param>
        /// <returns></returns>
        string CreateLocationMessage(int apiId, string deviceLocation);

        /// <summary>
        /// The broker topic for location messages
        /// </summary>
        /// <value>
        /// The location message topic.
        /// </value>
        string LocationMessageTopic { get; }

        /// <summary>
        /// Parses a LOCATION message from Helen's API as an apiId, deviceLocation pair.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Named Tuple containing the apiId and device location from the message, 
        ///     or null if the message couldn't be parsed</returns>
        (int apiId, string deviceLocation)? GetApiLocationFromMessage(string message);
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class HelenApiInterface : IHelenApiInterface
    {
        const int MAX_RETRIES = 3;
        private HttpClient client;
        private string HelenApiUrl;

        public HelenApiInterface(string helenApiUrl)
        {
            HelenApiUrl = helenApiUrl;

            client = new HttpClient
            {
                BaseAddress = new Uri(helenApiUrl)
            };
        }


        /// <summary>
        /// Gets the broker URL from Helen's WebAPI server
        /// </summary>
        /// <param name="defaultValue">The value to return if Helen's API can't be reached.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Connecting to Helen's server reached MAX_RETRIES
        /// or
        /// Helen's server didn't return a URL
        /// </exception>
        public string GetBrokerUrl(string defaultValue)
        {
            try
            {
                HttpResponseMessage response;
                int numTries = 0;
                do
                {
                    var route = "DeviceData/GetConnectionInfo/";
                    response = client.GetAsync(route).Result;
                    if (numTries++ > MAX_RETRIES)
                    {
                        throw new Exception("Connecting to Helen's server reached MAX_RETRIES");
                    }
                } while (!response.IsSuccessStatusCode);

                var msg = response.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(msg))
                {
                    throw new Exception("Helen's server didn't return a URL");
                }
                else
                {
                    return msg.Trim().Trim('\"');
                }
            }
            catch (Exception)
            {
                ConsoleExtensions.WriteLine("Failed to connect to Helen's API, falling back to the value in settings.");
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the API identifier from Helen's WebAPI server
        /// </summary>
        /// <param name="name">The name to register</param>
        /// <param name="defaultValue">The value to return if Helen's API can't be reached</param>
        /// <returns></returns>
        /// <exception cref="Exception">Connecting to Helen's server reached MAX_RETRIES</exception>
        public int GetApiId(string name, int defaultValue)
        {
            try
            {
                HttpResponseMessage response;
                int numTries = 0;
                do
                {
                    var route = "DeviceList/RegisterDevice/";
                    route += HttpUtility.UrlEncode(name);
                    response = client.PostAsJsonAsync(route, new JValue("")).Result;
                    if (numTries++ > MAX_RETRIES)
                    {
                        throw new Exception("Connecting to Helen's server reached MAX_RETRIES");
                    }
                } while (!response.IsSuccessStatusCode);

                var msg = response.Content.ReadAsStringAsync().Result;
                int id = int.Parse(msg);
                return id;
            }
            catch (Exception)
            {
                ConsoleExtensions.WriteLine("Failed to connect to Helen's API, falling back to the value in settings.");
                return defaultValue;
            }
        }

        /// <summary>
        /// Parses a LOCATION message from Helen's API as an apiId, deviceLocation pair.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public (int apiId, string deviceLocation)? GetApiLocationFromMessage(string message)
        {
            var parts = message.Split("-");
            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out int apiId))
                {
                    return (apiId, parts[1]);
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a data packet that Helen's MQTTManager can understand
        /// </summary>
        /// <param name="apiId">The API identifier.</param>
        /// <param name="data">The data to be sent.</param>
        /// <returns></returns>
        public string CreateDataMessage(int apiId, string data)
        {
            return $"{apiId}-{data}";
        }

        /// <summary>
        /// Creates a location packet that Helen's MQTTManager can understand
        /// </summary>
        /// <param name="apiId">The API identifier.</param>
        /// <param name="deviceLocation">The device location.</param>
        /// <returns></returns>
        public string CreateLocationMessage(int apiId, string deviceLocation)
        {
            return $"LOCATION-{apiId}-{deviceLocation}";
        }

        public string LocationMessageTopic => "Linux/locationUpdates";
    }
}
