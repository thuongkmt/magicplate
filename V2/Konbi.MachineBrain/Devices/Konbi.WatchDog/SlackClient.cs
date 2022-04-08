using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Konbi.WatchDog
{
    public class SlackClient
    {
        private readonly Uri _webHookUri;

        /// <summary>
        /// 
        /// </summary>
        public SlackClient(ILogger logger)
        {
            ConfigurationInfo configIinfo = new ConfigurationInfo();
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var currentPath = Path.GetDirectoryName(path);
            
            if (File.Exists($"{currentPath}\\monitoring.json"))
            {
                using (StreamReader r = new StreamReader($"{currentPath}\\monitoring.json"))
                {
                    string json = r.ReadToEnd();
                    configIinfo = JsonConvert.DeserializeObject<ConfigurationInfo>(json);
                }

                this._webHookUri = new Uri(configIinfo.SlackChannel.PostUrl);
            }
            else
            {
                logger.Information("Cannot found file monitoring.json.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void SendSlackMessage(SlackMessage message, ILogger logger)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                byte[] request = System.Text.Encoding.UTF8.GetBytes("payload=" + JsonConvert.SerializeObject(message));

                try
                {
                    byte[] response = webClient.UploadData(this._webHookUri, "POST", request);
                }
                catch (ArgumentNullException ex)
                {
                    logger.Information(ex.ToString());
                }
                catch (WebException ex)
                {
                    logger.Information(ex.ToString());
                }                
            }
        }
    }

    /// <summary>
    /// Struct slack message.
    /// </summary>
    public sealed class SlackMessage
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("icon_emoji")]
        public string Icon
        {
            get { return ":computer:"; }
        }
    }
}
