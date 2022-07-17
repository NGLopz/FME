using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FME.Repository.Utils
{
    public class JsonUtils
    {
        public static T DeserializeObjectOrDefault<T>(string json, T defaultValue)
        {
            if (json == null)
                json = "";

            var result = JsonConvert.DeserializeObject<T>(json);
            return result != null ? result : defaultValue;
        }
    }
}
