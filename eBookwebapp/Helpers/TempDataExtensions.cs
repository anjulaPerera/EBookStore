using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace eBookwebapp.Helpers
{
    public static class TempDataExtensions
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        public static void Set<T>(this ITempDataDictionary tempData, string key, T value)
        {
            tempData[key] = JsonConvert.SerializeObject(value, Settings);
        }

        public static T Get<T>(this ITempDataDictionary tempData, string key)
        {
            tempData.TryGetValue(key, out var value);
            return value == null ? default : JsonConvert.DeserializeObject<T>((string)value, Settings);
        }
    }
}
