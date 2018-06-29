using Service.Throttle.Helper;
using Service.Throttle.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Configuration;
using Newtonsoft.Json;

namespace Service.Throttle
{
    /// <summary>
    /// Initialize (load) throttle policies.
    /// </summary>
    public class ThrottleInit
    {
        /// <summary>
        ///  Load ClientThrollingPolicy from json data to cache memory.
        /// </summary>
        /// <param name="FolderName">Container folder of throttle data json file e.g. Resources</param>
        /// <param name="FileName">File name of throttle data json file e.g. ThrottleData.json</param>
        public static void CacheClientThrollingPolicy(string FolderName, string FileName)
        {
            try
            {
                var oList = new List<ClientRateLimiting>();
                var jsonfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FolderName, FileName);
                var jsontext = File.ReadAllText(jsonfile, Encoding.Default);
                List<Client> clients = JsonConvert.DeserializeObject<List<Client>>(jsontext);
                MemoryCacherHelper.Add("ClientThrollingPolicy", clients, DateTimeOffset.MaxValue);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }
    }
}
