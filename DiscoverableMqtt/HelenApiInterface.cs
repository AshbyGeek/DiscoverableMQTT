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
        string GetBrokerUrl(AppSettings settings);
        int GetApiId(AppSettings settings);
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class HelenApiInterface : IHelenApiInterface
    {
        const int MAX_RETRIES = 3;
        private static HttpClient client;

        public string GetBrokerUrl(AppSettings settings)
        {
            try
            {
                if (client == null)
                {
                    client = new HttpClient
                    {
                        BaseAddress = new Uri(settings.HelenApiUrl)
                    };
                }

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
                return settings.BrokerUrl;
            }
        }

        public int GetApiId(AppSettings settings)
        {
            try
            {
                if (client == null)
                {
                    client = new HttpClient
                    {
                        BaseAddress = new Uri(settings.HelenApiUrl)
                    };
                }

                HttpResponseMessage response;
                int numTries = 0;
                do
                {
                    var route = "DeviceList/RegisterDevice/";
                    route += HttpUtility.UrlEncode(settings.Name);
                    response = client.PostAsJsonAsync(route, new JValue("")).Result;
                    if (numTries++ > MAX_RETRIES)
                    {
                        throw new Exception("Connecting to Helen's server reached MAX_RETRIES");
                    }
                } while (!response.IsSuccessStatusCode);

                var msg = response.Content.ReadAsStringAsync().Result;
                var obj = JObject.Parse(msg);
                int id = (int)obj["id"];
                return id;
            }
            catch (Exception)
            {
                ConsoleExtensions.WriteLine("Failed to connect to Helen's API, falling back to the value in settings.");
                return settings.ApiId;
            }
        }
    }
}
