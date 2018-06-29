using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Service.WebAPI.Models;

namespace Service.WebAPI.Classes
{
    public static class DefaultParameters
    {
        static Dictionary<string, object> requestParameters = new Dictionary<string, object>();

        static DefaultParameters()
        {
            var jsonfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "tests.json");
            var jsontext = File.ReadAllText(jsonfile, Encoding.Default);
            var json = (dynamic)JObject.Parse(jsontext);
            foreach (var i in json.item)
            {
                if (i.request.method == "POST") requestParameters.Add((i.id + i.request.url).ToString(), (i.request.body.raw));
            }
        }

        public static object GetDefaultParam(string model)
        {
            foreach (var url in requestParameters)
            {
                if (url.Key.Contains("v1/AddEmployee") && model.Equals("Employee"))
                {
                    return ((JArray)requestParameters[url.Key]).First.ToObject<Employee>();
                }
            }
            return null;
        }

    }
}